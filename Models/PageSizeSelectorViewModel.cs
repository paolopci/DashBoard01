namespace DashboardOrders.Models;

public class PageSizeSelectorViewModel
{
    public string ActionName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = "Home";
    public int PageSize { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
    public Dictionary<string, string> ExtraRouteValues { get; set; } = new();

    public int[] PageSizeOptions { get; set; } = { 10, 20, 50, 0 };
}
