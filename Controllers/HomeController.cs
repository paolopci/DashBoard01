using DashboardOrders.Models.ViewModels;
using System.Diagnostics;
using DashboardOrders.Data;
using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StripeCheckoutSession = Stripe.Checkout.Session;
using StripeEvent = Stripe.Event;

namespace DashboardOrders.Controllers;

[Authorize]
public class HomeController : Controller
{
    private const string AdminRole = "Admin";
    private const string ToastSuccessKey = "Toast.Success";
    private const string ToastErrorKey = "Toast.Error";

    private readonly IDashboardOrdersDataService dataService;
    private readonly DashboardOrdersDbContext dbContext;
    private readonly IStripeCheckoutService stripeCheckoutService;

    public HomeController(
        IDashboardOrdersDataService dataService,
        DashboardOrdersDbContext dbContext,
        IStripeCheckoutService stripeCheckoutService)
    {
        this.dataService = dataService;
        this.dbContext = dbContext;
        this.stripeCheckoutService = stripeCheckoutService;
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
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Quantita non aggiornata. Verifica disponibilita articolo." });
            }

            TempData[ToastErrorKey] = "Quantita non aggiornata. Verifica disponibilita articolo.";
        }

        if (IsAjaxRequest())
        {
            return CartJson(productCode);
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
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Quantita non aggiornata. Verifica disponibilita articolo." });
            }

            TempData[ToastErrorKey] = "Quantita non aggiornata. Verifica disponibilita articolo.";
        }

        if (IsAjaxRequest())
        {
            return CartJson(productCode);
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
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Articolo non trovato nel carrello." });
            }

            TempData[ToastErrorKey] = "Articolo non trovato nel carrello.";
            return RedirectToAction(nameof(Cart));
        }

        if (item.Quantity <= 1)
        {
            dataService.RemoveCartItem(GetCurrentEmail(), productCode);
            if (IsAjaxRequest())
            {
                return CartJson(productCode, removedProductCode: productCode);
            }

            return RedirectToAction(nameof(Cart));
        }

        if (!dataService.UpdateCartItemQuantity(GetCurrentEmail(), productCode, item.Quantity - 1))
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Quantita non aggiornata. Verifica disponibilita articolo." });
            }

            TempData[ToastErrorKey] = "Quantita non aggiornata. Verifica disponibilita articolo.";
        }

        if (IsAjaxRequest())
        {
            return CartJson(productCode);
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
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Articolo non rimosso dal carrello." });
            }

            TempData[ToastErrorKey] = "Articolo non rimosso dal carrello.";
        }

        if (IsAjaxRequest())
        {
            return CartJson(productCode, removedProductCode: productCode);
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

        if (!dataService.StartCheckout(GetCurrentEmail()))
        {
            TempData[ToastErrorKey] = "Checkout non avviato. Verifica carrello e disponibilita articoli.";
            return RedirectToAction(nameof(Cart));
        }

        return RedirectToAction(nameof(CheckoutSummary));
    }

    [HttpGet]
    public IActionResult CheckoutSummary()
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var checkout = dataService.GetCheckout(GetCurrentEmail());
        if (checkout is null)
        {
            TempData[ToastErrorKey] = "Checkout non disponibile o scaduto.";
            return RedirectToAction(nameof(Cart));
        }

        return View(checkout);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StartCheckout()
    {
        return CheckoutCart();
    }

    [HttpGet]
    public IActionResult CheckoutAddresses()
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var checkout = dataService.GetCheckout(GetCurrentEmail());
        if (checkout is null)
        {
            TempData[ToastErrorKey] = "Checkout non disponibile o scaduto.";
            return RedirectToAction(nameof(Cart));
        }

        return View(checkout);
    }

    [HttpGet]
    public IActionResult ItalianProvinces()
    {
        return Json(dataService.GetItalianProvinces());
    }

    [HttpGet]
    public IActionResult ItalianCities(string? provinceName)
    {
        return string.IsNullOrWhiteSpace(provinceName)
            ? Json(Array.Empty<string>())
            : Json(dataService.GetItalianCities(provinceName));
    }

    [HttpGet]
    public IActionResult ItalianPostalCodes(string? provinceName, string? cityName)
    {
        return string.IsNullOrWhiteSpace(provinceName) || string.IsNullOrWhiteSpace(cityName)
            ? Json(Array.Empty<string>())
            : Json(dataService.GetItalianPostalCodes(provinceName, cityName));
    }

    [HttpGet]
    public IActionResult PhoneCountryPrefixes()
    {
        return Json(dataService.GetPhoneCountryPrefixes());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CheckoutAddresses(CheckoutAddressesViewModel model)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        if (!ModelState.IsValid)
        {
            var checkout = dataService.GetCheckout(GetCurrentEmail()) ?? new CheckoutSessionViewModel();
            CopyAddresses(model, checkout);
            return View(checkout);
        }

        if (!dataService.SaveCheckoutAddresses(GetCurrentEmail(), model))
        {
            ModelState.AddModelError(string.Empty, "Completa provincia, citta e CAP per spedizione e fatturazione.");
            var checkout = dataService.GetCheckout(GetCurrentEmail()) ?? new CheckoutSessionViewModel();
            CopyAddresses(model, checkout);
            return View(checkout);
        }

        return RedirectToAction(nameof(CheckoutConfirm));
    }

    [HttpGet]
    public IActionResult CheckoutConfirm()
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var checkout = dataService.GetCheckout(GetCurrentEmail());
        if (checkout is null)
        {
            TempData[ToastErrorKey] = "Checkout non disponibile o scaduto.";
            return RedirectToAction(nameof(Cart));
        }

        return View(checkout);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CheckoutOptions(CheckoutOptionsViewModel model)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        if (!ModelState.IsValid || !dataService.SaveCheckoutOptions(GetCurrentEmail(), model))
        {
            TempData[ToastErrorKey] = "Seleziona metodo consegna e pagamento validi.";
            return RedirectToAction(nameof(CheckoutConfirm));
        }

        return RedirectToAction(nameof(CheckoutConfirm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(CheckoutConfirm))]
    public async Task<IActionResult> CheckoutConfirmPost(CheckoutOptionsViewModel? model = null, CancellationToken cancellationToken = default)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        if (model is not null && ModelState.IsValid && !dataService.SaveCheckoutOptions(GetCurrentEmail(), model))
        {
            TempData[ToastErrorKey] = "Seleziona metodo consegna e pagamento validi.";
            return RedirectToAction(nameof(CheckoutConfirm));
        }

        var result = dataService.ConfirmCheckout(GetCurrentEmail());
        if (!result.Success || result.OrderId is null)
        {
            TempData[ToastErrorKey] = result.ErrorMessage;
            return RedirectToAction(nameof(CheckoutConfirm));
        }

        if (result.RequiresStripeCheckout)
        {
            var order = dataService.GetOrderDetails(result.OrderId.Value, GetCurrentEmail(), isAdmin: false);
            if (order is null)
            {
                TempData[ToastErrorKey] = "Ordine Stripe non trovato.";
                return RedirectToAction(nameof(CheckoutConfirm));
            }

            var returnUrl = Url.Action(nameof(StripeCheckoutReturn), "Home", null, Request.Scheme)
                + "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = Url.Action(nameof(CheckoutPayment), "Home", new { orderId = result.OrderId.Value }, Request.Scheme)
                ?? string.Empty;

            try
            {
                var stripeSession = await stripeCheckoutService.CreateCheckoutSessionAsync(order, returnUrl, cancelUrl, cancellationToken);
                if (!dataService.SaveStripeCheckoutSession(GetCurrentEmail(), result.OrderId.Value, stripeSession))
                {
                    TempData[ToastErrorKey] = "Sessione Stripe creata ma non salvata sull'ordine.";
                    return RedirectToAction(nameof(CheckoutPayment), new { orderId = result.OrderId.Value });
                }

                return Redirect(stripeSession.Url);
            }
            catch (Exception)
            {
                TempData[ToastErrorKey] = "Stripe Checkout non disponibile. Verifica configurazione test.";
                return RedirectToAction(nameof(CheckoutPayment), new { orderId = result.OrderId.Value });
            }
        }

        return result.RequiresPayment
            ? RedirectToAction(nameof(CheckoutPayment), new { orderId = result.OrderId.Value })
            : RedirectToAction(nameof(CheckoutResult), new { orderId = result.OrderId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> StripeCheckoutReturn(string? session_id, CancellationToken cancellationToken)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        if (string.IsNullOrWhiteSpace(session_id))
        {
            TempData[ToastErrorKey] = "Sessione Stripe mancante.";
            return RedirectToAction(nameof(Cart));
        }

        var orderId = dataService.GetOrderIdByStripeCheckoutSession(session_id);
        var stripeSession = await stripeCheckoutService.GetCheckoutSessionAsync(session_id, cancellationToken);
        if (stripeSession is null)
        {
            TempData[ToastErrorKey] = "Sessione Stripe non trovata.";
            return orderId.HasValue
                ? RedirectToAction(nameof(CheckoutPayment), new { orderId = orderId.Value })
                : RedirectToAction(nameof(Cart));
        }

        if (string.Equals(stripeSession.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            var result = dataService.CompleteStripePayment(stripeSession.SessionId, stripeSession.PaymentIntentId, stripeSession.PaymentStatus, GetCurrentEmail());
            if (result.Success && result.OrderId.HasValue)
            {
                return RedirectToAction(nameof(CheckoutResult), new { orderId = result.OrderId.Value });
            }

            TempData[ToastErrorKey] = result.ErrorMessage;
        }
        else
        {
            TempData[ToastErrorKey] = "Pagamento Stripe non completato.";
        }

        return orderId.HasValue
            ? RedirectToAction(nameof(CheckoutPayment), new { orderId = orderId.Value })
            : RedirectToAction(nameof(Cart));
    }

    [AllowAnonymous]
    [HttpPost("/stripe/webhook")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> StripeWebhook()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        StripeEvent stripeEvent;
        try
        {
            stripeEvent = stripeCheckoutService.ConstructWebhookEvent(payload, signature);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        if (stripeEvent.Data.Object is StripeCheckoutSession session)
        {
            if (stripeEvent.Type is "checkout.session.completed" or "checkout.session.async_payment_succeeded")
            {
                dataService.CompleteStripePayment(session.Id, session.PaymentIntentId, session.PaymentStatus, "stripe");
            }
            else if (stripeEvent.Type == "checkout.session.async_payment_failed")
            {
                dataService.FailStripePayment(session.Id, session.PaymentIntentId, session.PaymentStatus, "stripe");
            }
        }

        return Ok();
    }

    [HttpGet]
    public IActionResult CheckoutPayment(int orderId)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var order = dataService.GetOrderDetails(orderId, GetCurrentEmail(), isAdmin: false);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CheckoutPayment(int orderId, TestPaymentOutcome outcome)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var result = dataService.ProcessTestPayment(GetCurrentEmail(), orderId, outcome);
        if (!result.Success)
        {
            TempData[ToastErrorKey] = result.ErrorMessage;
            return RedirectToAction(nameof(CheckoutPayment), new { orderId });
        }

        return RedirectToAction(nameof(CheckoutResult), new { orderId });
    }

    [HttpGet]
    public IActionResult CheckoutResult(int orderId)
    {
        if (IsAdmin())
        {
            return RedirectToAction(nameof(Orders));
        }

        var order = dataService.GetOrderDetails(orderId, GetCurrentEmail(), isAdmin: false);
        return order is null ? NotFound() : View(order);
    }

    [HttpGet]
    public IActionResult OrderDetails(int id)
    {
        var order = dataService.GetOrderDetails(id, GetCurrentEmail(), IsAdmin());
        return order is null ? NotFound() : View(order);
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

    private static void CopyAddresses(CheckoutAddressesViewModel source, CheckoutSessionViewModel target)
    {
        target.ShippingLastName = source.ShippingLastName;
        target.ShippingFirstName = source.ShippingFirstName;
        target.ShippingPhonePrefix = source.ShippingPhonePrefix;
        target.ShippingPhoneCountryIso2 = source.ShippingPhoneCountryIso2;
        target.ShippingPhoneNumber = source.ShippingPhoneNumber;
        target.ShippingStreet = source.ShippingStreet;
        target.ShippingStreetNumber = source.ShippingStreetNumber;
        target.ShippingProvince = source.ShippingProvince;
        target.ShippingFullName = source.ShippingFullName;
        target.ShippingAddressLine = source.ShippingAddressLine;
        target.ShippingCity = source.ShippingCity;
        target.ShippingPostalCode = source.ShippingPostalCode;
        target.ShippingCountry = source.ShippingCountry;
        target.ShippingPhone = source.ShippingPhone;
        target.BillingSameAsShipping = source.BillingSameAsShipping;
        target.BillingFullName = source.BillingFullName;
        target.BillingAddressLine = source.BillingAddressLine;
        target.BillingCity = source.BillingCity;
        target.BillingPostalCode = source.BillingPostalCode;
        target.BillingCountry = source.BillingCountry;
        target.BillingVatNumber = source.BillingVatNumber;
    }

    private bool IsAdmin()
    {
        return User.IsInRole(AdminRole);
    }

    private string? GetCurrentEmail()
    {
        return User.Identity?.Name;
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    private JsonResult CartJson(string productCode, string? removedProductCode = null)
    {
        var cart = dataService.GetCart(GetCurrentEmail());
        var item = cart.Items.FirstOrDefault(existing => string.Equals(existing.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));

        return Json(new
        {
            success = true,
            removedProductCode,
            cart = new
            {
                totalItems = cart.TotalItems,
                totalAmount = DisplayFormatter.FormatEuro(cart.TotalAmount),
                hasUnavailableItems = cart.HasUnavailableItems
            },
            item = item is null
                ? null
                : new
                {
                    productCode = item.ProductCode,
                    quantity = item.Quantity,
                    lineTotal = DisplayFormatter.FormatEuro(item.LineTotal),
                    isAvailable = item.IsAvailable
                }
        });
    }
}
