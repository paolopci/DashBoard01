namespace DashboardOrders.Domain.Entities;

public class ProductCarouselImageEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public ProductEntity Product { get; set; } = null!;
}
