using DashboardOrders.Models.ViewModels;
using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.ViewComponents;

public class CartBadgeViewComponent(IDashboardOrdersDataService dataService) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var user = HttpContext.User;
        if (user.Identity?.IsAuthenticated != true || user.IsInRole("Admin"))
        {
            return Content(string.Empty);
        }

        return View(new CartBadgeViewModel
        {
            ItemsCount = dataService.GetCartItemsCount(user.Identity.Name)
        });
    }
}
