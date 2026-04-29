namespace DashboardOrders.Models.ViewModels;

public class CategoryPageViewModel
{
    public List<Category> Categories { get; set; } = new();
    public int TotalCategories { get; set; }
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";
    public bool HasCategories => TotalCategories > 0;
}
