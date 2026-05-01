namespace DashboardOrders.Domain.Entities;

public class ProductEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public CategoryEntity Category { get; set; } = null!;
    public ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
    public ICollection<ProductCarouselImageEntity> CarouselImages { get; set; } = new List<ProductCarouselImageEntity>();
}
