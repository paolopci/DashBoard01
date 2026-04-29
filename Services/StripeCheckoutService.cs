using DashboardOrders.Models;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace DashboardOrders.Services;

public class StripeCheckoutService : IStripeCheckoutService
{
    private readonly StripeCheckoutOptions options;

    public StripeCheckoutService(IOptions<StripeCheckoutOptions> options)
    {
        this.options = options.Value;
    }

    public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        OrderDetailsViewModel orderDetails,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var order = orderDetails.Order;
        if (order.Id <= 0 || order.Items.Count == 0)
        {
            throw new InvalidOperationException("Ordine non valido per Stripe Checkout.");
        }

        var createOptions = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = order.Id.ToString(),
            CustomerEmail = order.Customer.Email,
            Metadata = new Dictionary<string, string>
            {
                ["order_id"] = order.Id.ToString(),
                ["order_number"] = order.OrderNumber,
                ["customer_email"] = order.Customer.Email
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = order.Id.ToString(),
                    ["order_number"] = order.OrderNumber,
                    ["customer_email"] = order.Customer.Email
                }
            },
            LineItems = order.Items.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = NormalizeCurrency(options.Currency),
                    UnitAmount = ToStripeAmount(item.UnitPrice),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = string.IsNullOrWhiteSpace(item.ProductName) ? "Articolo ordine" : item.ProductName
                    }
                }
            }).ToList()
        };

        var session = await CreateSessionService().CreateAsync(createOptions, cancellationToken: cancellationToken);
        return MapSession(session);
    }

    public async Task<StripeCheckoutSessionResult?> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        var session = await CreateSessionService().GetAsync(sessionId, cancellationToken: cancellationToken);
        return MapSession(session);
    }

    public Event ConstructWebhookEvent(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(options.WebhookSecret))
        {
            throw new InvalidOperationException("Stripe:WebhookSecret non configurata. Usa User Secrets, variabili d'ambiente o un secret store sicuro.");
        }

        return EventUtility.ConstructEvent(payload, signature, options.WebhookSecret);
    }

    private static StripeCheckoutSessionResult MapSession(Session session)
    {
        return new StripeCheckoutSessionResult
        {
            SessionId = session.Id,
            Url = session.Url ?? string.Empty,
            PaymentIntentId = session.PaymentIntentId,
            PaymentStatus = session.PaymentStatus ?? string.Empty
        };
    }

    private SessionService CreateSessionService()
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            throw new InvalidOperationException("Stripe:SecretKey non configurata. Usa User Secrets, variabili d'ambiente o un secret store sicuro.");
        }

        return new SessionService(new StripeClient(options.SecretKey));
    }

    private static long ToStripeAmount(decimal amount)
    {
        return decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
    }

    private static string NormalizeCurrency(string? currency)
    {
        var normalized = (currency ?? string.Empty).Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? "eur" : normalized;
    }
}
