using Microsoft.Extensions.Options;

namespace DashboardOrders.Extensions.Auth;

public static class CorsExtensions
{
    public static IServiceCollection AddDashboardCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));

        var corsOptions = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddPolicy(corsOptions.PolicyName, policy =>
            {
                if (corsOptions.AllowedOrigins.Length == 0)
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => false);

                    return;
                }

                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseDashboardCors(this IApplicationBuilder app)
    {
        var corsOptions = app.ApplicationServices.GetRequiredService<IOptions<CorsOptions>>().Value;
        return app.UseCors(corsOptions.PolicyName);
    }
}
