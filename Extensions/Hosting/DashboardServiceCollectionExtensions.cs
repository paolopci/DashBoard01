using DashboardOrders.Data;
using DashboardOrders.Extensions.Auth;
using DashboardOrders.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Extensions.Hosting;

public static class DashboardServiceCollectionExtensions
{
    public static IServiceCollection AddDashboardApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDashboardCors(configuration);
        services.AddDashboardDbContext(configuration);

        services.AddScoped<IDashboardOrdersDataService, DashboardOrdersDataService>();
        services.AddScoped<IDashboardAnalyticsService, DashboardAnalyticsService>();
        services.AddScoped<DashboardOrdersDatabaseSeeder>();
        services.AddScoped<ItalianTerritoryImporter>();
        services.Configure<StripeCheckoutOptions>(configuration.GetSection("Stripe"));
        services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();
        services.AddScoped<IAccountService, AccountService>();

        services.AddDashboardIdentity();
        services.AddDashboardJwtAuthentication(configuration, environment);

        return services;
    }

    private static IServiceCollection AddDashboardDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DashboardAppDb")
            ?? throw new InvalidOperationException("Connection string 'DashboardAppDb' non configurata. Usa .NET User Secrets, variabili d'ambiente o un secret store sicuro.");
        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            TrustServerCertificate = true
        };

        services.AddDbContext<DashboardOrdersDbContext>(options =>
            options.UseSqlServer(
                sqlConnectionStringBuilder.ConnectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        return services;
    }
}
