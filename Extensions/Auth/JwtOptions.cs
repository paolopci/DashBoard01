namespace DashboardOrders.Extensions.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    private const string DevelopmentOnlySecretKey = "development-only-dashboard-orders-jwt-key-change-before-production";

    public string Issuer { get; set; } = "DashboardOrders";

    public string Audience { get; set; } = "DashboardOrders";

    public string SecretKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;

    public static JwtOptions Resolve(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var jwtOptions = configuration.GetSection(SectionName).Get<JwtOptions>() ?? new JwtOptions();

        if (!string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
        {
            return jwtOptions;
        }

        if (environment.IsDevelopment())
        {
            jwtOptions.SecretKey = DevelopmentOnlySecretKey;
            return jwtOptions;
        }

        throw new InvalidOperationException("Jwt:SecretKey non configurata. Usa user secrets, variabili d'ambiente o un secret store sicuro.");
    }
}
