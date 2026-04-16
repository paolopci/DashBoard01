using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DashboardOrders.Models;

public class CustomerSearchFormViewModel
{
    private const string SearchTermKey = "CustomerSearchTerm";
    private const string PageSizeKey = "CustomerPageSize";
    private const string SortByKey = "CustomerSortBy";
    private const string SortDirectionKey = "CustomerSortDirection";

    public bool IsEnabled { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "totalAmount";
    public string SortDirection { get; set; } = "desc";

    public static CustomerSearchFormViewModel From(CustomersPageViewModel model)
    {
        return new CustomerSearchFormViewModel
        {
            IsEnabled = true,
            SearchTerm = model.SearchTerm,
            PageSize = model.PageSize,
            SortBy = model.SortBy,
            SortDirection = model.SortDirection
        };
    }

    public static CustomerSearchFormViewModel FromViewData(ViewDataDictionary viewData, bool isEnabled)
    {
        return new CustomerSearchFormViewModel
        {
            IsEnabled = isEnabled,
            SearchTerm = viewData[SearchTermKey]?.ToString() ?? string.Empty,
            PageSize = ReadInt(viewData, PageSizeKey, 10),
            SortBy = viewData[SortByKey]?.ToString() ?? "totalAmount",
            SortDirection = viewData[SortDirectionKey]?.ToString() ?? "desc"
        };
    }

    public void ApplyTo(ViewDataDictionary viewData)
    {
        viewData[SearchTermKey] = SearchTerm;
        viewData[PageSizeKey] = PageSize;
        viewData[SortByKey] = SortBy;
        viewData[SortDirectionKey] = SortDirection;
    }

    private static int ReadInt(ViewDataDictionary viewData, string key, int fallback)
    {
        return int.TryParse(viewData[key]?.ToString(), out var value)
            ? value
            : fallback;
    }
}
