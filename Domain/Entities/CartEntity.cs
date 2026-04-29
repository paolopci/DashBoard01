namespace DashboardOrders.Domain.Entities;

public class CartEntity
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public ICollection<CartItemEntity> Items { get; set; } = new List<CartItemEntity>();
}
