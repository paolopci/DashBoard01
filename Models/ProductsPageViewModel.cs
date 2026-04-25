namespace DashboardOrders.Models;

public class ProductsPageViewModel : PagedPageViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalStock { get; set; }
    public decimal InventoryValue { get; set; }
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";
    public string SelectedCategoryCode { get; set; } = string.Empty;
}
