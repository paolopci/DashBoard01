using DashboardOrders.Data;
using DashboardOrders.Extensions.Auth;
using DashboardOrders.Services;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Extensions.Hosting;

public static class DashboardWebApplicationExtensions
{
    public static void UseDashboardRequestPipeline(this WebApplication app)
    {
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
    }

    public static async Task<bool> TryRunDashboardCommandAsync(this WebApplication app, string[] args)
    {
        if (HasArgument(args, "--seed-database"))
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DashboardOrdersDatabaseSeeder>();
            await seeder.SeedAsync();

            return true;
        }

        if (HasArgument(args, "--import-italian-territories"))
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

            return true;
        }

        if (HasArgument(args, "--verify-italian-territories"))
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

            return true;
        }

        return false;
    }

    private static bool HasArgument(string[] args, string argumentName)
    {
        return args.Any(arg => string.Equals(arg, argumentName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetRequiredArgumentValue(string[] args, string argumentName)
    {
        return GetOptionalArgumentValue(args, argumentName)
            ?? throw new InvalidOperationException($"Argomento obbligatorio mancante: {argumentName}");
    }

    private static string? GetOptionalArgumentValue(string[] args, string argumentName)
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

    private static async Task<ItalianTerritoryVerificationResult> VerifyItalianTerritoriesAsync(DashboardOrdersDbContext dbContext)
    {
        var regions = await dbContext.ItalianRegions.CountAsync();
        var provinces = await dbContext.ItalianProvinces.CountAsync();
        var municipalities = await dbContext.ItalianMunicipalities.CountAsync();
        var postalCodes = await dbContext.ItalianPostalCodes.CountAsync();

        // Verifica che ogni comune punti a provincia e regione esistenti, evitando dati territoriali scollegati.
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

        // La Sardegna usa un assetto UTS specifico: il controllo intercetta import parziali o dataset non allineati.
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

    private static async Task RequireMunicipalityAsync(DashboardOrdersDbContext dbContext, string code, string name)
    {
        var exists = await dbContext.ItalianMunicipalities
            .AnyAsync(municipality => municipality.Code == code && municipality.Name == name);
        RequireTrue(exists, $"Comune ISTAT atteso non trovato: {name} ({code}).");
    }

    private static void RequireEqual(int expected, int actual, string message)
    {
        if (expected != actual)
        {
            throw new InvalidOperationException($"{message} Atteso: {expected}; trovato: {actual}.");
        }
    }

    private static void RequireTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed record ItalianTerritoryVerificationResult(
        int Regions,
        int Provinces,
        int Municipalities,
        int PostalCodes,
        int MunicipalityOrphans,
        int ProvinceOrphans);
}
