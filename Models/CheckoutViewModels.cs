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
    [Required(ErrorMessage = "Indica nome e cognome per la spedizione.")]
    [StringLength(120)]
    public string ShippingFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica l'indirizzo di spedizione.")]
    [StringLength(200)]
    public string ShippingAddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica la citta di spedizione.")]
    [StringLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica il CAP di spedizione.")]
    [StringLength(20)]
    public string ShippingPostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica il paese di spedizione.")]
    [StringLength(100)]
    public string ShippingCountry { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica un telefono di contatto.")]
    [StringLength(30)]
    public string ShippingPhone { get; set; } = string.Empty;

    public bool BillingSameAsShipping { get; set; } = true;

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
