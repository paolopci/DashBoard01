namespace DashboardOrders.Services;

public class StripeCheckoutOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "eur";
}
