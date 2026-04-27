namespace DashboardOrders.Data.Entities;

public class CheckoutSessionEntity
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingFullName { get; set; }
    public string? ShippingAddressLine { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPhone { get; set; }
    public bool BillingSameAsShipping { get; set; }
    public string? BillingFullName { get; set; }
    public string? BillingAddressLine { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingVatNumber { get; set; }
    public string DeliveryMethod { get; set; } = "standard";
    public string PaymentMethod { get; set; } = "pending";
    public int? CreatedOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
