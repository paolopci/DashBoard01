using DashboardOrders.Models.ViewModels;
using DashboardOrders.Models;
using Stripe;

namespace DashboardOrders.Services;

public interface IStripeCheckoutService
{
    Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        OrderDetailsViewModel orderDetails,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<StripeCheckoutSessionResult?> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Event ConstructWebhookEvent(string payload, string signature);
}
