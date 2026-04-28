using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models;

public enum CheckoutStep
{
    Summary = 0,
    Addresses = 1,
    Confirm = 2,
    Payment = 3,
    Result = 4
}

public enum TestPaymentOutcome
{
    Authorized = 0,
    Failed = 1,
    Pending = 2
}

public class CheckoutAddressesViewModel
{
    [StringLength(80)]
    public string ShippingLastName { get; set; } = string.Empty;

    [StringLength(80)]
    public string ShippingFirstName { get; set; } = string.Empty;

    [StringLength(8)]
    public string ShippingPhonePrefix { get; set; } = string.Empty;

    [StringLength(30)]
    public string ShippingPhoneNumber { get; set; } = string.Empty;

    [StringLength(160)]
    public string ShippingStreet { get; set; } = string.Empty;

    [StringLength(20)]
    public string ShippingStreetNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string ShippingProvince { get; set; } = string.Empty;

    [StringLength(120)]
    public string ShippingFullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string ShippingAddressLine { get; set; } = string.Empty;

    [StringLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [StringLength(20)]
    public string ShippingPostalCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string ShippingCountry { get; set; } = string.Empty;

    [StringLength(30)]
    public string ShippingPhone { get; set; } = string.Empty;

    public bool BillingSameAsShipping { get; set; } = true;

    [StringLength(80)]
    public string BillingLastName { get; set; } = string.Empty;

    [StringLength(80)]
    public string BillingFirstName { get; set; } = string.Empty;

    [StringLength(160)]
    public string BillingStreet { get; set; } = string.Empty;

    [StringLength(20)]
    public string BillingStreetNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string BillingProvince { get; set; } = string.Empty;

    [StringLength(120)]
    public string BillingFullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string BillingAddressLine { get; set; } = string.Empty;

    [StringLength(100)]
    public string BillingCity { get; set; } = string.Empty;

    [StringLength(20)]
    public string BillingPostalCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string BillingCountry { get; set; } = string.Empty;

    [StringLength(40)]
    public string BillingVatNumber { get; set; } = string.Empty;
}

public class CheckoutOptionsViewModel
{
    [Required]
    public string DeliveryMethod { get; set; } = "standard";

    [Required]
    public string PaymentMethod { get; set; } = "pending";
}

public class CheckoutSessionViewModel : CheckoutAddressesViewModel
{
    public CheckoutStep CurrentStep { get; set; }
    public CartViewModel Cart { get; set; } = new();
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryMethod { get; set; } = "standard";
    public string PaymentMethod { get; set; } = "pending";
    public int? CreatedOrderId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CheckoutConfirmResult
{
    public bool Success { get; set; }
    public int? OrderId { get; set; }
    public bool RequiresPayment { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static CheckoutConfirmResult Failed(string errorMessage)
    {
        return new CheckoutConfirmResult { ErrorMessage = errorMessage };
    }
}

public class CheckoutPaymentResult
{
    public bool Success { get; set; }
    public int? OrderId { get; set; }
    public OrderStatus? FinalStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class OrderDetailsViewModel
{
    public Order Order { get; set; } = new();
    public OrderCheckoutDetailsViewModel? CheckoutDetails { get; set; }
}

public class OrderCheckoutDetailsViewModel
{
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
    public string BillingVatNumber { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string TestTransactionReference { get; set; } = string.Empty;
}
