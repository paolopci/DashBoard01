namespace DashboardOrders.Data.Entities;

public class OrderStatusHistoryEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? FromStatus { get; set; }
    public int ToStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? CorrelationId { get; set; }

    public OrderEntity Order { get; set; } = null!;
}
