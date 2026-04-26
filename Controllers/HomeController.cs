using System.Diagnostics;
using DashboardOrders.Data;
using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

[Authorize]
public class HomeController : Controller
{
    private const string AdminRole = "Admin";
    private const string ToastSuccessKey = "Toast.Success";
    private const string ToastErrorKey = "Toast.Error";

    private readonly IDashboardOrdersDataService dataService;
    private readonly DashboardOrdersDbContext dbContext;

    public HomeController(IDashboardOrdersDataService dataService, DashboardOrdersDbContext dbContext)
    {
        this.dataService = dataService;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Recupera i dati del dashboard e li visualizza.
    /// </summary>
    /// <param name="page">Numero della pagina corrente.</param>
    /// <param name="pageSize">Numero di elementi per pagina.</param>
    /// <param name="sortBy">Campo di ordinamento.</param>
    /// <param name="sortDirection">Direzione dell'ordinamento.</param>
    /// <param name="search">Termine di ricerca facoltativo.</param>
    public IActionResult Dashboard(string period = "30d")
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var analyticsService = new DashboardAnalyticsService(dbContext);
        var model = analyticsService.GetAnalytics(period);
        return View(model);
    }

public IActionResult Index(int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "")
    {
        if (!IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var model = dataService.GetDashboardData(page, pageSize, sortBy, sortDirection, search);
        SearchFormViewModel.From(model, "Index", model.SortBy, model.SortDirection).ApplyTo(ViewData);
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
    /// <param name="search">Termine di ricerca facoltativo.</param>
    public IActionResult Orders(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "")
    {
        var model = IsAdmin()
            ? dataService.GetOrdersPageData(customerId, page, pageSize, sortBy, sortDirection, search, dateFrom, dateTo)
            : dataService.GetOrdersPageDataForCustomerEmail(GetCurrentEmail(), page, pageSize, sortBy, sortDirection, search, dateFrom, dateTo);
        SearchFormViewModel
            .From(model, "Orders", sortBy, sortDirection, new Dictionary<string, string>
            {
                ["customerId"] = IsAdmin() ? customerId?.ToString() ?? string.Empty : string.Empty,
                ["dateFrom"] = model.DateFrom,
                ["dateTo"] = model.DateTo
            })
            .ApplyTo(ViewData);
        ViewData["DateFrom"] = model.DateFrom;
        ViewData["DateTo"] = model.DateTo;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeOrderStatus(int orderId, OrderStatus newStatus, string? reason = null)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (orderId <= 0 || !Enum.IsDefined(typeof(OrderStatus), newStatus))
        {
            TempData[ToastErrorKey] = "Cambio stato non valido.";
            return RedirectToAction(nameof(Orders));
        }

        if (!dataService.ChangeOrderStatus(orderId, newStatus, GetCurrentEmail(), reason))
        {
            TempData[ToastErrorKey] = "Stato ordine non aggiornato. Verifica transizione e motivazione.";
            return RedirectToAction(nameof(Orders));
        }

        TempData[ToastSuccessKey] = "Stato ordine aggiornato correttamente.";
        return RedirectToAction(nameof(Orders));
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
        if (!IsAdmin())
        {
            return RedirectToAction("Profile", "Account");
        }

        var model = dataService.GetCustomersPageData(page, pageSize, sortBy, sortDirection, search);
        SearchFormViewModel.From(model, "Customers", model.SortBy, model.SortDirection).ApplyTo(ViewData);
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
    /// <param name="search">Termine di ricerca facoltativo.</param>
    public IActionResult Products(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "")
    {
        var model = dataService.GetProductsPageData(page, pageSize, sortBy, sortDirection, categoryCode, search);
        SearchFormViewModel
            .From(model, "Products", sortBy, sortDirection, new Dictionary<string, string>
            {
                ["categoryCode"] = categoryCode
            })
            .ApplyTo(ViewData);
        return View(model);
    }

    [HttpGet]
    public IActionResult NewOrder()
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        return View(CreateNewOrderViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NewOrder(NewOrderViewModel? model)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        model ??= new NewOrderViewModel();
        var productCode = string.IsNullOrWhiteSpace(model.ProductCode)
            ? model.Items.FirstOrDefault()?.ProductCode ?? string.Empty
            : model.ProductCode;
        var quantity = model.Quantity ?? model.Items.FirstOrDefault()?.Quantity ?? 0;

        if (string.IsNullOrWhiteSpace(productCode) || quantity <= 0)
        {
            ModelState.AddModelError(string.Empty, "Seleziona un articolo e indica una quantita valida.");
        }

        if (!ModelState.IsValid)
        {
            PopulateNewOrderLookups(model);
            return View(model);
        }

        if (!dataService.AddOrUpdateCartItem(GetCurrentEmail(), productCode, quantity))
        {
            ModelState.AddModelError(string.Empty, "Articolo non aggiunto al carrello. Verifica prodotto e quantita disponibile.");
            PopulateNewOrderLookups(model);
            return View(model);
        }

        TempData[ToastSuccessKey] = "Articolo aggiunto al carrello.";
        return RedirectToAction(nameof(NewOrder));
    }

    [HttpGet]
    public IActionResult Cart()
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        return View(dataService.GetCart(GetCurrentEmail()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCartItemQuantity(string productCode, int quantity)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        if (!dataService.UpdateCartItemQuantity(GetCurrentEmail(), productCode, quantity))
        {
            TempData[ToastErrorKey] = "Quantita non aggiornata. Verifica disponibilita articolo.";
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult IncrementCartItem(string productCode)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var item = dataService.GetCart(GetCurrentEmail()).Items.FirstOrDefault(existing => string.Equals(existing.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));
        if (item is null || !dataService.UpdateCartItemQuantity(GetCurrentEmail(), productCode, item.Quantity + 1))
        {
            TempData[ToastErrorKey] = "Quantita non aggiornata. Verifica disponibilita articolo.";
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DecrementCartItem(string productCode)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var item = dataService.GetCart(GetCurrentEmail()).Items.FirstOrDefault(existing => string.Equals(existing.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));
        if (item is null)
        {
            TempData[ToastErrorKey] = "Articolo non trovato nel carrello.";
            return RedirectToAction(nameof(Cart));
        }

        if (item.Quantity <= 1)
        {
            dataService.RemoveCartItem(GetCurrentEmail(), productCode);
            return RedirectToAction(nameof(Cart));
        }

        if (!dataService.UpdateCartItemQuantity(GetCurrentEmail(), productCode, item.Quantity - 1))
        {
            TempData[ToastErrorKey] = "Quantita non aggiornata. Verifica disponibilita articolo.";
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCartItem(string productCode)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        if (!dataService.RemoveCartItem(GetCurrentEmail(), productCode))
        {
            TempData[ToastErrorKey] = "Articolo non rimosso dal carrello.";
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CheckoutCart()
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var cart = dataService.GetCart(GetCurrentEmail());
        if (cart.Items.Count == 0)
        {
            TempData[ToastErrorKey] = "Il carrello e vuoto.";
            return RedirectToAction(nameof(Cart));
        }

        if (cart.HasUnavailableItems)
        {
            TempData[ToastErrorKey] = "Verifica le quantita: uno o piu articoli non sono disponibili.";
            return RedirectToAction(nameof(Cart));
        }

        var items = cart.Items
            .Select(item => new NewOrderItemViewModel
            {
                ProductCode = item.ProductCode,
                Quantity = item.Quantity
            })
            .ToList();

        if (!dataService.CreateOrder(GetCurrentEmail(), items))
        {
            TempData[ToastErrorKey] = "Ordine non creato. Verifica prodotto e quantita disponibile.";
            return RedirectToAction(nameof(Cart));
        }

        dataService.ClearCart(GetCurrentEmail());
        TempData[ToastSuccessKey] = "Ordine creato correttamente.";
        return RedirectToAction(nameof(Orders));
    }

    [HttpGet]
    public IActionResult EditCustomer(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var customer = dataService.GetCustomer(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditCustomer(Customer? model)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        model ??= new Customer();
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!dataService.UpdateCustomer(model))
        {
            return NotFound();
        }

        TempData[ToastSuccessKey] = "Cliente aggiornato correttamente.";
        return RedirectToAction(nameof(Customers));
    }

    [HttpGet]
    public IActionResult CreateProduct()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return View(CreateProductFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateProduct(ProductFormViewModel? model)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        model ??= new ProductFormViewModel();
        if (!ModelState.IsValid)
        {
            model.Categories = dataService.GetCategoryPageData().Categories;
            return View(model);
        }

        if (!dataService.CreateProduct(ToProduct(model)))
        {
            ModelState.AddModelError(string.Empty, "Prodotto non creato. Verifica codice e categoria.");
            model.Categories = dataService.GetCategoryPageData().Categories;
            return View(model);
        }

        TempData[ToastSuccessKey] = "Prodotto creato correttamente.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public IActionResult EditProduct(string code)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var product = dataService.GetProduct(code);
        return product is null ? NotFound() : View(ToProductFormViewModel(product));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditProduct(ProductFormViewModel? model)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        model ??= new ProductFormViewModel();
        if (!ModelState.IsValid)
        {
            model.Categories = dataService.GetCategoryPageData().Categories;
            return View(model);
        }

        if (!dataService.UpdateProduct(ToProduct(model)))
        {
            return NotFound();
        }

        TempData[ToastSuccessKey] = "Prodotto aggiornato correttamente.";
        return RedirectToAction(nameof(Products));
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        Response.StatusCode = statusCode ?? Response.StatusCode;
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private NewOrderViewModel CreateNewOrderViewModel()
    {
        var model = new NewOrderViewModel();
        PopulateNewOrderLookups(model);
        return model;
    }

    private void PopulateNewOrderLookups(NewOrderViewModel model)
    {
        model.Products = dataService.GetAvailableProducts();
        model.Categories = model.Products
            .Select(product => product.Category)
            .GroupBy(category => category.Code, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(category => category.Name)
            .ToList();
    }

    private ProductFormViewModel CreateProductFormViewModel()
    {
        return new ProductFormViewModel
        {
            Categories = dataService.GetCategoryPageData().Categories
        };
    }

    private ProductFormViewModel ToProductFormViewModel(Product product)
    {
        return new ProductFormViewModel
        {
            Code = product.Code,
            Name = product.Name,
            CategoryCode = product.Category.Code,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            UnitCost = product.UnitCost,
            Stock = product.Stock,
            Categories = dataService.GetCategoryPageData().Categories
        };
    }

    private static Product ToProduct(ProductFormViewModel model)
    {
        return new Product
        {
            Code = model.Code,
            Name = model.Name,
            Category = new Category { Code = model.CategoryCode },
            Description = model.Description,
            ImageUrl = model.ImageUrl,
            UnitCost = model.UnitCost,
            Stock = model.Stock
        };
    }

    private bool IsAdmin()
    {
        return User.IsInRole(AdminRole);
    }

    private string? GetCurrentEmail()
    {
        return User.Identity?.Name;
    }
}
