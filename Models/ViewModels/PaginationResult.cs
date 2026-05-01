namespace DashboardOrders.Models.ViewModels;

public record PaginationResult<T>(
    List<T> Items,
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
}