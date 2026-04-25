namespace DashboardOrders.Models;

public enum OrderStatus
{
    // Preserve persisted legacy values already stored in Orders.Status.
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
    Cart = 5,
    PaymentPending = 6,
    PaymentAuthorized = 7,
    PaymentFailed = 8,
    Confirmed = 9,
    Picking = 10,
    Packing = 11,
    ReturnRequested = 12,
    ReturnApproved = 13,
    Returned = 14,
    Refunded = 15,
    PartiallyRefunded = 16
}
