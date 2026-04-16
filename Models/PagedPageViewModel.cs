namespace DashboardOrders.Models;

public abstract class PagedPageViewModel
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool ShowAll => PageSize == 0;
    public bool HasPreviousPage => !ShowAll && CurrentPage > 1;
    public bool HasNextPage => !ShowAll && CurrentPage < TotalPages;
    public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;
    public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;
}
