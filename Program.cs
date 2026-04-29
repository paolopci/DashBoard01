using System.Reflection;
using DashboardOrders.Data;
using DashboardOrders.Extensions.Auth;
using DashboardOrders.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDashboardCors(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DashboardAppDb")
    ?? throw new InvalidOperationException("Connection string 'DashboardAppDb' non configurata. Usa .NET User Secrets, variabili d'ambiente o un secret store sicuro.");
var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
{
    TrustServerCertificate = true
};
builder.Services.AddDbContext<DashboardOrdersDbContext>(options =>
    options.UseSqlServer(
        sqlConnectionStringBuilder.ConnectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IDashboardOrdersDataService, DashboardOrdersDataService>();
builder.Services.AddScoped<DashboardOrdersDatabaseSeeder>();
builder.Services.AddScoped<ItalianTerritoryImporter>();
builder.Services.Configure<StripeCheckoutOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddDashboardIdentity();
builder.Services.AddDashboardJwtAuthentication(builder.Configuration, builder.Environment);

var app = builder.Build();

if (args.Any(arg => string.Equals(arg, "--seed-database", StringComparison.OrdinalIgnoreCase)))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DashboardOrdersDatabaseSeeder>();
    await seeder.SeedAsync();

    return;
}

if (args.Any(arg => string.Equals(arg, "--import-italian-territories", StringComparison.OrdinalIgnoreCase)))
{
    using var scope = app.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<ItalianTerritoryImporter>();
    var result = await importer.ImportAsync(new ItalianTerritoryImportOptions
    {
        IstatMunicipalitiesXlsxPath = GetRequiredArgumentValue(args, "--istat-territories-xlsx"),
        PostalCodesCsvPath = GetOptionalArgumentValue(args, "--italian-postal-codes-csv")
    });

    app.Logger.LogInformation(
        "Italian territory import completed. Regions: {Regions}, provinces: {Provinces}, municipalities: {Municipalities}, postal codes: {PostalCodes}, skipped postal codes: {SkippedPostalCodes}.",
        result.RegionsImported,
        result.ProvincesImported,
        result.MunicipalitiesImported,
        result.PostalCodesImported,
        result.PostalCodesSkipped);

    return;
}

if (args.Any(arg => string.Equals(arg, "--verify-italian-territories", StringComparison.OrdinalIgnoreCase)))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DashboardOrdersDbContext>();
    var result = await VerifyItalianTerritoriesAsync(dbContext);

    app.Logger.LogInformation(
        "Italian territory verification completed. Regions: {Regions}, provinces: {Provinces}, municipalities: {Municipalities}, postal codes: {PostalCodes}, municipality orphans: {MunicipalityOrphans}, province orphans: {ProvinceOrphans}.",
        result.Regions,
        result.Provinces,
        result.Municipalities,
        result.PostalCodes,
        result.MunicipalityOrphans,
        result.ProvinceOrphans);

    return;
}

app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseDashboardCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static string GetRequiredArgumentValue(string[] args, string argumentName)
{
    return GetOptionalArgumentValue(args, argumentName)
        ?? throw new InvalidOperationException($"Argomento obbligatorio mancante: {argumentName}");
}

static string? GetOptionalArgumentValue(string[] args, string argumentName)
{
    for (var index = 0; index < args.Length - 1; index++)
    {
        if (string.Equals(args[index], argumentName, StringComparison.OrdinalIgnoreCase))
        {
            return args[index + 1];
        }
    }

    return null;
}

static async Task<ItalianTerritoryVerificationResult> VerifyItalianTerritoriesAsync(DashboardOrdersDbContext dbContext)
{
    var regions = await dbContext.ItalianRegions.CountAsync();
    var provinces = await dbContext.ItalianProvinces.CountAsync();
    var municipalities = await dbContext.ItalianMunicipalities.CountAsync();
    var postalCodes = await dbContext.ItalianPostalCodes.CountAsync();

    var municipalityOrphans = await dbContext.ItalianMunicipalities
        .CountAsync(municipality =>
            !dbContext.ItalianProvinces.Any(province => province.Code == municipality.ProvinceCode) ||
            !dbContext.ItalianRegions.Any(region => region.Code == municipality.RegionCode));
    var provinceOrphans = await dbContext.ItalianProvinces
        .CountAsync(province => !dbContext.ItalianRegions.Any(region => region.Code == province.RegionCode));

    RequireEqual(20, regions, "Conteggio regioni ISTAT non valido.");
    RequireEqual(110, provinces, "Conteggio province/citta metropolitane/UTS ISTAT non valido.");
    RequireEqual(7894, municipalities, "Conteggio comuni ISTAT non valido.");
    RequireEqual(8457, postalCodes, "Conteggio CAP importati non valido.");
    RequireEqual(0, municipalityOrphans, "Sono presenti comuni senza provincia o regione collegata.");
    RequireEqual(0, provinceOrphans, "Sono presenti province senza regione collegata.");

    await RequireMunicipalityAsync(dbContext, "058091", "Roma");
    await RequireMunicipalityAsync(dbContext, "015146", "Milano");
    await RequireMunicipalityAsync(dbContext, "041044", "Pesaro");
    await RequireMunicipalityAsync(dbContext, "024129", "Castegnero Nanto");

    string[] sardinianUnitCodes = ["113", "114", "115", "116", "117", "119", "312", "318"];
    var sardinianUnits = await dbContext.ItalianProvinces
        .CountAsync(province => province.RegionCode == "20" && sardinianUnitCodes.Contains(province.Code));
    var sardinianMunicipalities = await dbContext.ItalianMunicipalities
        .CountAsync(municipality => municipality.RegionCode == "20");

    RequireEqual(sardinianUnitCodes.Length, sardinianUnits, "Assetto UTS Sardegna 2026 non coerente.");
    RequireEqual(377, sardinianMunicipalities, "Conteggio comuni Sardegna non valido.");

    var placeholderPostalCodes = await dbContext.ItalianPostalCodes
        .CountAsync(postalCode => postalCode.PostalCode == "00000" || postalCode.CityName == "999999");
    RequireEqual(0, placeholderPostalCodes, "Sono presenti CAP placeholder o non verificati.");

    return new ItalianTerritoryVerificationResult(
        regions,
        provinces,
        municipalities,
        postalCodes,
        municipalityOrphans,
        provinceOrphans);
}

static async Task RequireMunicipalityAsync(DashboardOrdersDbContext dbContext, string code, string name)
{
    var exists = await dbContext.ItalianMunicipalities
        .AnyAsync(municipality => municipality.Code == code && municipality.Name == name);
    RequireTrue(exists, $"Comune ISTAT atteso non trovato: {name} ({code}).");
}

static void RequireEqual(int expected, int actual, string message)
{
    if (expected != actual)
    {
        throw new InvalidOperationException($"{message} Atteso: {expected}; trovato: {actual}.");
    }
}

static void RequireTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

internal sealed record ItalianTerritoryVerificationResult(
    int Regions,
    int Provinces,
    int Municipalities,
    int PostalCodes,
    int MunicipalityOrphans,
    int ProvinceOrphans);
