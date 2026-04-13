using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

public class HomeController : Controller
{
    /// <summary>
    /// Recupera i dati del dashboard e li visualizza.
    /// </summary>
    public IActionResult Index()
    {
        var model = MockDataService.GetDashboardData();
        return View(model);
    }

    /// <summary>
    /// Recupera paginando gli ordini e li visualizza.
    /// </summary>
    /// <param name="customerId">Identificatore del cliente (facoltativo).</param>
    /// <param name="page">Numero della pagina corrente.</param>
    /// <param name="pageSize">Numero di elementi per pagina.</param>
    /// <param name="sortBy">Campo di ordinamento.</param>
    /// <param name="sortDirection">Direzione dell'ordinamento.</param>
    public IActionResult Orders(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc")
    {
        var model = MockDataService.GetOrdersPageData(customerId, page, pageSize, sortBy, sortDirection);
        return View(model);
    }

    /// <summary>
    /// Recupera paginando i clienti e li visualizza.
    /// </summary>
    /// <param name="page">Numero della pagina corrente.</param>
    /// <param name="pageSize">Numero di elementi per pagina.</param>
    /// <param name="sortBy">Campo di ordinamento.</param>
    /// <param name="sortDirection">Direzione dell'ordinamento.</param>
    /// <param name="search">Termine di ricerca facoltativo.</param>
    public IActionResult Customers(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
    {
        var model = MockDataService.GetCustomersPageData(page, pageSize, sortBy, sortDirection, search);
        ViewData["CustomerSearchTerm"] = model.SearchTerm;
        ViewData["CustomerPageSize"] = model.PageSize;
        ViewData["CustomerSortBy"] = model.SortBy;
        ViewData["CustomerSortDirection"] = model.SortDirection;
        return View(model);
    }

    /// <summary>
    /// Recupera paginando i prodotti e li visualizza.
    /// </summary>
    /// <param name="page">Numero della pagina corrente.</param>
    /// <param name="pageSize">Numero di elementi per pagina.</param>
    /// <param name="sortBy">Campo di ordinamento.</param>
    /// <param name="sortDirection">Direzione dell'ordinamento.</param>
    /// <param name="categoryCode">Codice della categoria (facoltativo).</param>
    public IActionResult Products(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "")
    {
        var model = MockDataService.GetProductsPageData(page, pageSize, sortBy, sortDirection, categoryCode);
        return View(model);
    }
}
