namespace DashboardOrders.Models;

public class Product
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Category Category { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public int Stock { get; set; }
}
