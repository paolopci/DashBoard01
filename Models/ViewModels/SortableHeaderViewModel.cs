namespace DashboardOrders.Models.ViewModels;

public class SortableHeaderViewModel
{
    public string ActionName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = "Home";
    public string Column { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string CurrentSortBy { get; set; } = string.Empty;
    public string CurrentSortDirection { get; set; } = string.Empty;
    public int PageSize { get; set; }
    public string Alignment { get; set; } = "left";
    public Dictionary<string, string> ExtraRouteValues { get; set; } = new();

    public string NextSortDirection => CurrentSortBy == Column && CurrentSortDirection == "asc" ? "desc" : "asc";
    public string SortArrow => CurrentSortBy == Column ? (CurrentSortDirection == "asc" ? "↑" : "↓") : "↑↓";

    public string SortLabelClass => CurrentSortBy == Column
        ? "font-bold text-slate-800"
        : "font-semibold text-slate-500";

    public string AlignmentClass => Alignment switch
    {
        "center" => "justify-center",
        "right" => "justify-end",
        _ => string.Empty
    };

    public Dictionary<string, string> RouteValues()
    {
        var routeValues = ExtraRouteValues
            .Where(routeValue => !string.IsNullOrWhiteSpace(routeValue.Value))
            .ToDictionary(routeValue => routeValue.Key, routeValue => routeValue.Value);

        routeValues["page"] = "1";
        routeValues["pageSize"] = PageSize.ToString();
        routeValues["sortBy"] = Column;
        routeValues["sortDirection"] = NextSortDirection;

        return routeValues;
    }
}
