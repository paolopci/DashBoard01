namespace DashboardOrders.Models;

public class PaginationViewModel
{
    public string ActionName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = "Home";
    public string EntityLabel { get; set; } = "elementi";
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int DisplayedItems { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
    public Dictionary<string, string> ExtraRouteValues { get; set; } = new();

    public bool ShowAll => PageSize == 0;
    public bool HasPreviousPage => !ShowAll && CurrentPage > 1;
    public bool HasNextPage => !ShowAll && CurrentPage < TotalPages;
    public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;
    public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;

    public IEnumerable<int> VisiblePages
    {
        get
        {
            if (ShowAll)
            {
                return Enumerable.Empty<int>();
            }

            var pages = new List<int> { 1 };

            for (var pageNumber = Math.Max(2, CurrentPage - 1); pageNumber <= Math.Min(TotalPages - 1, CurrentPage + 1); pageNumber++)
            {
                if (!pages.Contains(pageNumber))
                {
                    pages.Add(pageNumber);
                }
            }

            if (TotalPages > 1 && !pages.Contains(TotalPages))
            {
                pages.Add(TotalPages);
            }

            return pages
                .Where(pageNumber => pageNumber >= 1 && pageNumber <= TotalPages)
                .Distinct()
                .OrderBy(pageNumber => pageNumber);
        }
    }

    public Dictionary<string, string> RouteValuesForPage(int page)
    {
        var routeValues = BuildBaseRouteValues();
        routeValues["page"] = page.ToString();
        return routeValues;
    }

    public Dictionary<string, string> PageSelectorRouteValues()
    {
        var routeValues = BuildBaseRouteValues();
        routeValues.Remove("page");
        return routeValues;
    }

    private Dictionary<string, string> BuildBaseRouteValues()
    {
        var routeValues = ExtraRouteValues
            .Where(routeValue => !string.IsNullOrWhiteSpace(routeValue.Value))
            .ToDictionary(routeValue => routeValue.Key, routeValue => routeValue.Value);

        routeValues["pageSize"] = PageSize.ToString();
        routeValues["sortBy"] = SortBy;
        routeValues["sortDirection"] = SortDirection;

        return routeValues;
    }
}
