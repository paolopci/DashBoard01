using DashboardOrders.Data;
using DashboardOrders.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secret.json", optional: true, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DashboardAppDb")
    ?? throw new InvalidOperationException("Connection string 'DashboardAppDb' non configurata. Crea secret.json con la connection string locale.");
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

var app = builder.Build();

if (args.Any(arg => string.Equals(arg, "--seed-database", StringComparison.OrdinalIgnoreCase)))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DashboardOrdersDatabaseSeeder>();
    await seeder.SeedAsync();

    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
