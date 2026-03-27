namespace DashboardOrders.Models;

public class ProductsPageViewModel
{
    public List<Product> Products { get; set; } = new();
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalStock { get; set; }
    public decimal InventoryValue { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool ShowAll => PageSize == 0;
    public bool HasPreviousPage => !ShowAll && CurrentPage > 1;
    public bool HasNextPage => !ShowAll && CurrentPage < TotalPages;
    public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;
    public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;
}
