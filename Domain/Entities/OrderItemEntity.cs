namespace DashboardOrders.Domain.Entities;

public class OrderItemEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public OrderEntity Order { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}
