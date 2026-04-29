namespace DashboardOrders.Domain.Entities;

public class CategoryEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
}
