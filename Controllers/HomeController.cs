using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var model = MockDataService.GetDashboardData();
        return View(model);
    }

    public IActionResult Orders(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc")
    {
        var model = MockDataService.GetOrdersPageData(customerId, page, pageSize, sortBy, sortDirection);
        return View(model);
    }

    public IActionResult Customers(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
    {
        var model = MockDataService.GetCustomersPageData(page, pageSize, sortBy, sortDirection, search);
        ViewData["CustomerSearchTerm"] = model.SearchTerm;
        ViewData["CustomerPageSize"] = model.PageSize;
        ViewData["CustomerSortBy"] = model.SortBy;
        ViewData["CustomerSortDirection"] = model.SortDirection;
        return View(model);
    }
}
