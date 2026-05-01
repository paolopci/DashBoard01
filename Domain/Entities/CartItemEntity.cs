namespace DashboardOrders.Domain.Entities;

public class CartItemEntity
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CartEntity Cart { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}
