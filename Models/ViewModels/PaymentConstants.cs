namespace DashboardOrders.Models.ViewModels;

public static class PaymentConstants
{
    public const string MethodPending = "pending";
    public const string MethodTestCard = "test-card";
    public const string MethodStripeTest = "stripe-test";

    public const string StatusPending = "pending";
    public const string StatusAuthorized = "authorized";
    public const string StatusFailed = "failed";
    public const string StatusNotRequired = "not-required";
    public const string StripeStatusPaid = "paid";
}
