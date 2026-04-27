namespace DashboardOrders.Data.Entities;

public class OrderCheckoutDetailsEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public bool BillingSameAsShipping { get; set; }
    public string BillingFullName { get; set; } = string.Empty;
    public string BillingAddressLine { get; set; } = string.Empty;
    public string BillingCity { get; set; } = string.Empty;
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string? BillingVatNumber { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TestTransactionReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public OrderEntity Order { get; set; } = null!;
}
