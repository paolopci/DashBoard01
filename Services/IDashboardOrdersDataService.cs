using DashboardOrders.Models;

namespace DashboardOrders.Services;

public interface IDashboardOrdersDataService
{
    DashboardViewModel GetDashboardData(int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "");
    OrdersPageViewModel GetOrdersPageData(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "");
    CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "");
    ProductsPageViewModel GetProductsPageData(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "");
    CategoryPageViewModel GetCategoryPageData(string sortBy = "code", string sortDirection = "asc");
    Category? GetCategory(string code);
    bool CreateCategory(Category category);
    bool UpdateCategory(Category category);
    bool DeleteCategory(string code);
}
