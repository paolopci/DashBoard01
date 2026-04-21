using System.Linq.Expressions;

namespace DashboardOrders.Models;

public class FilterCriteria<T>
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "contains"; // contains, eq, gt, lt, date_range, etc.
    public object? Value { get; set; }
    public List<FilterCriteria<T>>? Children { get; set; } // For complex AND/OR groups

    public static Expression<Func<T, bool>> BuildExpression(FilterCriteria<T> criteria, ParameterExpression param)
    {
        // Recursive builder for complex filters
        // Implement based on Operator and Field (using reflection or switch)
        throw new NotImplementedException("Expression builder to be implemented");
    }
}