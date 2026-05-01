namespace DashboardOrders.Domain.Entities;

public class CustomerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string AvatarInitials { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
}
