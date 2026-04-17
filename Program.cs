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
