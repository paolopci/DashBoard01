using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DashboardOrders.Models;

public class SearchFormViewModel
{
    private const string IsEnabledKey = "SearchFormIsEnabled";
    private const string ActionNameKey = "SearchFormActionName";
    private const string SearchTermKey = "SearchFormSearchTerm";
    private const string PageSizeKey = "SearchFormPageSize";
    private const string SortByKey = "SearchFormSortBy";
    private const string SortDirectionKey = "SearchFormSortDirection";
    private const string ExtraRouteValuesKey = "SearchFormExtraRouteValues";

    public bool IsEnabled { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = "Home";
    public string SearchTerm { get; set; } = string.Empty;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
    public Dictionary<string, string> ExtraRouteValues { get; set; } = new();

    public static SearchFormViewModel From(
        PagedPageViewModel model,
        string actionName,
        string sortBy,
        string sortDirection,
        Dictionary<string, string>? extraRouteValues = null)
    {
        return new SearchFormViewModel
        {
            IsEnabled = true,
            ActionName = actionName,
            SearchTerm = model.SearchTerm,
            PageSize = model.PageSize,
            SortBy = sortBy,
            SortDirection = sortDirection,
            ExtraRouteValues = extraRouteValues ?? new Dictionary<string, string>()
        };
    }

    public static SearchFormViewModel FromViewData(ViewDataDictionary viewData)
    {
        return new SearchFormViewModel
        {
            IsEnabled = ReadBool(viewData, IsEnabledKey),
            ActionName = viewData[ActionNameKey]?.ToString() ?? string.Empty,
            SearchTerm = viewData[SearchTermKey]?.ToString() ?? string.Empty,
            PageSize = ReadInt(viewData, PageSizeKey, 10),
            SortBy = viewData[SortByKey]?.ToString() ?? string.Empty,
            SortDirection = viewData[SortDirectionKey]?.ToString() ?? string.Empty,
            ExtraRouteValues = ReadRouteValues(viewData)
        };
    }

    public void ApplyTo(ViewDataDictionary viewData)
    {
        viewData[IsEnabledKey] = IsEnabled;
        viewData[ActionNameKey] = ActionName;
        viewData[SearchTermKey] = SearchTerm;
        viewData[PageSizeKey] = PageSize;
        viewData[SortByKey] = SortBy;
        viewData[SortDirectionKey] = SortDirection;
        viewData[ExtraRouteValuesKey] = ExtraRouteValues;
    }

    private static bool ReadBool(ViewDataDictionary viewData, string key)
    {
        return bool.TryParse(viewData[key]?.ToString(), out var value) && value;
    }

    private static int ReadInt(ViewDataDictionary viewData, string key, int fallback)
    {
        return int.TryParse(viewData[key]?.ToString(), out var value)
            ? value
            : fallback;
    }

    private static Dictionary<string, string> ReadRouteValues(ViewDataDictionary viewData)
    {
        return viewData[ExtraRouteValuesKey] is Dictionary<string, string> routeValues
            ? routeValues
            : new Dictionary<string, string>();
    }
}
