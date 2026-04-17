namespace DashboardOrders.Extensions.Auth;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string PolicyName { get; set; } = "DashboardCorsPolicy";

    public string[] AllowedOrigins { get; set; } = [];
}
