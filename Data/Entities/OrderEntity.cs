namespace DashboardOrders.Data.Entities;

public class OrderEntity
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public CustomerEntity Customer { get; set; } = null!;
    public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
    public ICollection<OrderStatusHistoryEntity> StatusHistory { get; set; } = new List<OrderStatusHistoryEntity>();
    public OrderCheckoutDetailsEntity? CheckoutDetails { get; set; }
}
