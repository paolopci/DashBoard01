namespace DashboardOrders.Models;

public class DashboardViewModel
{
    public List<Order> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int ActiveCustomers { get; set; }
    public string SortBy { get; set; } = "date";
    public string SortDirection { get; set; } = "desc";
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool ShowAll => PageSize == 0;
    public bool HasPreviousPage => !ShowAll && CurrentPage > 1;
    public bool HasNextPage => !ShowAll && CurrentPage < TotalPages;
    public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;
    public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;
}
