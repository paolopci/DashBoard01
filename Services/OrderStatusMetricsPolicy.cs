using DashboardOrders.Models;

namespace DashboardOrders.Services;

public static class OrderStatusMetricsPolicy
{
    public static bool IsRevenueRelevant(OrderStatus status)
    {
        return status is not OrderStatus.Cancelled
            and not OrderStatus.PaymentFailed
            and not OrderStatus.Refunded;
    }

    public static bool IsOperationallyActive(OrderStatus status)
    {
        return status is OrderStatus.Pending
            or OrderStatus.PaymentPending
            or OrderStatus.PaymentAuthorized
            or OrderStatus.Confirmed
            or OrderStatus.Processing
            or OrderStatus.Picking
            or OrderStatus.Packing;
    }

    public static bool IsFulfillmentCompleted(OrderStatus status)
    {
        return status is OrderStatus.Shipped or OrderStatus.Delivered;
    }
}
