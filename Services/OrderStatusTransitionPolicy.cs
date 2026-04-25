using DashboardOrders.Models;

namespace DashboardOrders.Services;

public static class OrderStatusTransitionPolicy
{
    private static readonly IReadOnlyDictionary<OrderStatus, IReadOnlySet<OrderStatus>> AllowedTransitions =
        new Dictionary<OrderStatus, IReadOnlySet<OrderStatus>>
        {
            [OrderStatus.Cart] = new HashSet<OrderStatus>
            {
                OrderStatus.Pending,
                OrderStatus.Cancelled
            },
            [OrderStatus.Pending] = new HashSet<OrderStatus>
            {
                OrderStatus.PaymentPending,
                OrderStatus.Confirmed,
                OrderStatus.Cancelled
            },
            [OrderStatus.PaymentPending] = new HashSet<OrderStatus>
            {
                OrderStatus.PaymentAuthorized,
                OrderStatus.PaymentFailed,
                OrderStatus.Cancelled
            },
            [OrderStatus.PaymentAuthorized] = new HashSet<OrderStatus>
            {
                OrderStatus.Confirmed,
                OrderStatus.Cancelled
            },
            [OrderStatus.PaymentFailed] = new HashSet<OrderStatus>
            {
                OrderStatus.PaymentPending,
                OrderStatus.Cancelled
            },
            [OrderStatus.Confirmed] = new HashSet<OrderStatus>
            {
                OrderStatus.Processing,
                OrderStatus.Cancelled
            },
            [OrderStatus.Processing] = new HashSet<OrderStatus>
            {
                OrderStatus.Picking
            },
            [OrderStatus.Picking] = new HashSet<OrderStatus>
            {
                OrderStatus.Packing
            },
            [OrderStatus.Packing] = new HashSet<OrderStatus>
            {
                OrderStatus.Shipped
            },
            [OrderStatus.Shipped] = new HashSet<OrderStatus>
            {
                OrderStatus.Delivered
            },
            [OrderStatus.Delivered] = new HashSet<OrderStatus>
            {
                OrderStatus.ReturnRequested,
                OrderStatus.PartiallyRefunded
            },
            [OrderStatus.ReturnRequested] = new HashSet<OrderStatus>
            {
                OrderStatus.ReturnApproved
            },
            [OrderStatus.ReturnApproved] = new HashSet<OrderStatus>
            {
                OrderStatus.Returned
            },
            [OrderStatus.Returned] = new HashSet<OrderStatus>
            {
                OrderStatus.Refunded,
                OrderStatus.PartiallyRefunded
            },
            [OrderStatus.PartiallyRefunded] = new HashSet<OrderStatus>(),
            [OrderStatus.Refunded] = new HashSet<OrderStatus>(),
            [OrderStatus.Cancelled] = new HashSet<OrderStatus>()
        };

    public static bool CanTransition(OrderStatus fromStatus, OrderStatus toStatus)
    {
        return AllowedTransitions.TryGetValue(fromStatus, out var nextStatuses)
            && nextStatuses.Contains(toStatus);
    }

    public static bool IsTerminal(OrderStatus status)
    {
        return status is OrderStatus.Cancelled or OrderStatus.Refunded or OrderStatus.PartiallyRefunded;
    }

    public static bool RequiresReason(OrderStatus status)
    {
        return status is OrderStatus.Cancelled
            or OrderStatus.PaymentFailed
            or OrderStatus.Refunded
            or OrderStatus.PartiallyRefunded;
    }
}
