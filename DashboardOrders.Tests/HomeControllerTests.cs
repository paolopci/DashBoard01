using DashboardOrders.Controllers;
using DashboardOrders.Data;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace DashboardOrders.Tests;

public class HomeControllerTests
{
    private readonly IDashboardOrdersDataService dataService;
    private readonly HomeController sut;
    private readonly DashboardOrdersDbContext dbContext;

    public HomeControllerTests()
    {
        dataService = Substitute.For<IDashboardOrdersDataService>();
        dbContext = new DashboardOrdersDbContext(new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase($"HomeControllerTests-{Guid.NewGuid()}")
            .Options);
        sut = new HomeController(dataService, dbContext)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser("admin@micene.it", "Admin")
                }
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public void Index_QuandoServizioRestituisceDashboard_AlloraRestituisceVistaConModello()
    {
        // Arrange
        var modelloAtteso = new DashboardViewModel { SortBy = "date", SortDirection = "desc" };
        dataService.GetDashboardData(1, 10, "date", "desc", string.Empty).Returns(modelloAtteso);

        // Act
        var risultato = sut.Index();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
    }

    [Fact]
    public void Index_QuandoRichiesto_AlloraInvocaServizioUnaVolta()
    {
        // Arrange
        dataService.GetDashboardData(1, 10, "date", "desc", string.Empty).Returns(new DashboardViewModel());

        // Act
        sut.Index();

        // Assert
        dataService.Received(1).GetDashboardData(1, 10, "date", "desc", string.Empty);
    }

    [Fact]
    public void Index_QuandoUtenteNonAdmin_AlloraReindirizzaAOrders()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("giulia.lombardi65@example.com");

        // Act
        var risultato = sut.Index();

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Orders");
        dataService.DidNotReceive().GetDashboardData(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Index_QuandoUtenteAdmin_AlloraMostraDashboard()
    {
        // Arrange
        var modelloAtteso = new DashboardViewModel { SortBy = "date", SortDirection = "desc" };
        dataService.GetDashboardData(1, 10, "date", "desc", string.Empty).Returns(modelloAtteso);

        // Act
        var risultato = sut.Index();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
    }

    [Fact]
    public void Orders_QuandoClienteSelezionato_AlloraRestituisceVistaConModello()
    {
        // Arrange
        var modelloAtteso = new OrdersPageViewModel
        {
            SortBy = "date",
            SortDirection = "desc",
            SelectedCustomerId = 7
        };
        dataService.GetOrdersPageData(7, 1, 10, "date", "desc", string.Empty).Returns(modelloAtteso);

        // Act
        var risultato = sut.Orders(customerId: 7);

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
    }

    [Fact]
    public void Orders_QuandoFiltriDataIndicati_AlloraInvocaServizioConIntervalloDate()
    {
        // Arrange
        const string dateFrom = "2026-04-01";
        const string dateTo = "2026-04-20";
        var modelloAtteso = new OrdersPageViewModel
        {
            SortBy = "date",
            SortDirection = "desc",
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        dataService.GetOrdersPageData(null, 1, 10, "date", "desc", string.Empty, dateFrom, dateTo).Returns(modelloAtteso);

        // Act
        var risultato = sut.Orders(dateFrom: dateFrom, dateTo: dateTo);

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
        dataService.Received(1).GetOrdersPageData(null, 1, 10, "date", "desc", string.Empty, dateFrom, dateTo);
        sut.ViewData["DateFrom"].Should().Be(dateFrom);
        sut.ViewData["DateTo"].Should().Be(dateTo);
    }

    [Fact]
    public void Customers_QuandoRicercaNull_AlloraInvocaServizioConRicercaNull()
    {
        // Arrange
        string search = null!;
        dataService.GetCustomersPageData(1, 10, "totalAmount", "desc", search).Returns(new CustomersPageViewModel());

        // Act
        sut.Customers(search: search);

        // Assert
        dataService.Received(1).GetCustomersPageData(1, 10, "totalAmount", "desc", search);
    }

    [Fact]
    public void Products_QuandoCategoriaIndicata_AlloraRestituisceVistaConModello()
    {
        // Arrange
        var modelloAtteso = new ProductsPageViewModel
        {
            SortBy = "code",
            SortDirection = "asc",
            SelectedCategoryCode = "CAT-001"
        };
        dataService.GetProductsPageData(1, 10, "code", "asc", "CAT-001", string.Empty).Returns(modelloAtteso);

        // Act
        var risultato = sut.Products(categoryCode: "CAT-001");

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
    }

    [Fact]
    public void Orders_QuandoUtenteNonAdmin_AlloraInvocaServizioConEmailUtente()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.GetOrdersPageDataForCustomerEmail("mario.rossi@example.com", 1, 10, "date", "desc", string.Empty)
            .Returns(new OrdersPageViewModel());

        // Act
        sut.Orders();

        // Assert
        dataService.Received(1).GetOrdersPageDataForCustomerEmail("mario.rossi@example.com", 1, 10, "date", "desc", string.Empty);
        dataService.DidNotReceive().GetOrdersPageData(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Orders_QuandoUtenteNonAdminEFiltriDataIndicati_AlloraInvocaServizioClienteConIntervalloDate()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        const string dateFrom = "2026-04-01";
        const string dateTo = "2026-04-20";
        dataService.GetOrdersPageDataForCustomerEmail("mario.rossi@example.com", 1, 10, "date", "desc", string.Empty, dateFrom, dateTo)
            .Returns(new OrdersPageViewModel { DateFrom = dateFrom, DateTo = dateTo });

        // Act
        sut.Orders(dateFrom: dateFrom, dateTo: dateTo);

        // Assert
        dataService.Received(1).GetOrdersPageDataForCustomerEmail("mario.rossi@example.com", 1, 10, "date", "desc", string.Empty, dateFrom, dateTo);
        dataService.DidNotReceive().GetOrdersPageData(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void ChangeOrderStatus_QuandoAdminETransizioneValida_AlloraInvocaServizioEReindirizzaAOrders()
    {
        // Arrange
        dataService.ChangeOrderStatus(12, OrderStatus.PaymentPending, "admin@micene.it", "Avvio pagamento", null)
            .Returns(true);

        // Act
        var risultato = sut.ChangeOrderStatus(12, OrderStatus.PaymentPending, "Avvio pagamento");

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Orders");
        dataService.Received(1).ChangeOrderStatus(12, OrderStatus.PaymentPending, "admin@micene.it", "Avvio pagamento", null);
        sut.TempData["Toast.Success"].Should().Be("Stato ordine aggiornato correttamente.");
    }

    [Fact]
    public void ChangeOrderStatus_QuandoUtenteNonAdmin_AlloraReindirizzaAccessDeniedENonInvocaServizio()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");

        // Act
        var risultato = sut.ChangeOrderStatus(12, OrderStatus.PaymentPending, "Avvio pagamento");

        // Assert
        var redirect = risultato.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("AccessDenied");
        redirect.ControllerName.Should().Be("Account");
        dataService.DidNotReceive().ChangeOrderStatus(Arg.Any<int>(), Arg.Any<OrderStatus>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    [Theory]
    [InlineData(0, OrderStatus.PaymentPending)]
    [InlineData(-1, OrderStatus.PaymentPending)]
    [InlineData(12, (OrderStatus)999)]
    public void ChangeOrderStatus_QuandoInputNonValido_AlloraReindirizzaAOrdersENonInvocaServizio(int orderId, OrderStatus newStatus)
    {
        // Act
        var risultato = sut.ChangeOrderStatus(orderId, newStatus, "Motivo");

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Orders");
        dataService.DidNotReceive().ChangeOrderStatus(Arg.Any<int>(), Arg.Any<OrderStatus>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>());
        sut.TempData["Toast.Error"].Should().Be("Cambio stato non valido.");
    }

    [Fact]
    public void ChangeOrderStatus_QuandoServizioFallisce_AlloraReindirizzaAOrdersConErrore()
    {
        // Arrange
        dataService.ChangeOrderStatus(12, OrderStatus.Delivered, "admin@micene.it", "Salto non ammesso", null)
            .Returns(false);

        // Act
        var risultato = sut.ChangeOrderStatus(12, OrderStatus.Delivered, "Salto non ammesso");

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Orders");
        dataService.Received(1).ChangeOrderStatus(12, OrderStatus.Delivered, "admin@micene.it", "Salto non ammesso", null);
        sut.TempData["Toast.Error"].Should().Be("Stato ordine non aggiornato. Verifica transizione e motivazione.");
    }

    [Fact]
    public void Customers_QuandoUtenteNonAdmin_AlloraReindirizzaAlProfilo()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");

        // Act
        var risultato = sut.Customers();

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Profile");
    }

    [Fact]
    public void NewOrder_Get_QuandoUtenteNonAdmin_AlloraMostraProdottiDisponibili()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        var prodotti = new List<Product>
        {
            new()
            {
                Code = "PRD-001",
                Name = "Laptop tracer",
                Category = new Category { Code = "CAT-001", Name = "Informatica" },
                UnitCost = 25m,
                Stock = 5
            }
        };
        dataService.GetAvailableProducts().Returns(prodotti);

        // Act
        var risultato = sut.NewOrder();

        // Assert
        var model = risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<NewOrderViewModel>().Subject;
        model.Products.Should().BeSameAs(prodotti);
        model.Categories.Should().ContainSingle(category => category.Code == "CAT-001");
    }

    [Fact]
    public void NewOrder_Post_QuandoUtenteNonAdminEArticoloValido_AlloraAggiungeAlCarrelloEReindirizzaANewOrder()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        var model = new NewOrderViewModel
        {
            ProductCode = "PRD-001",
            Quantity = 2
        };
        dataService.AddOrUpdateCartItem("mario.rossi@example.com", "PRD-001", 2).Returns(true);

        // Act
        var risultato = sut.NewOrder(model);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("NewOrder");
        dataService.Received(1).AddOrUpdateCartItem("mario.rossi@example.com", "PRD-001", 2);
        dataService.DidNotReceive().CreateOrder(Arg.Any<string?>(), Arg.Any<IReadOnlyList<NewOrderItemViewModel>>());
        sut.TempData["Toast.Success"].Should().Be("Articolo aggiunto al carrello.");
    }

    [Fact]
    public void NewOrder_Post_QuandoNessunArticolo_AlloraMostraErroreENonAggiungeAlCarrello()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.GetAvailableProducts().Returns([]);
        var model = new NewOrderViewModel();

        // Act
        var risultato = sut.NewOrder(model);

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(model);
        sut.ModelState.IsValid.Should().BeFalse();
        dataService.DidNotReceive().AddOrUpdateCartItem(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public void Cart_QuandoUtenteNonAdmin_AlloraMostraCarrello()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        var modelloAtteso = new CartViewModel();
        dataService.GetCart("mario.rossi@example.com").Returns(modelloAtteso);

        // Act
        var risultato = sut.Cart();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
    }

    [Fact]
    public void Cart_QuandoAdmin_AlloraReindirizzaAOrders()
    {
        // Act
        var risultato = sut.Cart();

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Orders");
        dataService.DidNotReceive().GetCart(Arg.Any<string?>());
    }

    [Fact]
    public void UpdateCartItemQuantity_QuandoCliente_AlloraAggiornaQuantitaEReindirizzaACart()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.UpdateCartItemQuantity("mario.rossi@example.com", "PRD-001", 3).Returns(true);

        // Act
        var risultato = sut.UpdateCartItemQuantity("PRD-001", 3);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Cart");
        dataService.Received(1).UpdateCartItemQuantity("mario.rossi@example.com", "PRD-001", 3);
    }

    [Fact]
    public void RemoveCartItem_QuandoCliente_AlloraRimuoveArticoloEReindirizzaACart()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.RemoveCartItem("mario.rossi@example.com", "PRD-001").Returns(true);

        // Act
        var risultato = sut.RemoveCartItem("PRD-001");

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Cart");
        dataService.Received(1).RemoveCartItem("mario.rossi@example.com", "PRD-001");
    }

    [Fact]
    public void IncrementCartItem_QuandoClienteEArticoloPresente_AlloraIncrementaQuantita()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.GetCart("mario.rossi@example.com").Returns(new CartViewModel
        {
            Items = [new CartItemViewModel { ProductCode = "PRD-001", Quantity = 2, Stock = 5 }]
        });
        dataService.UpdateCartItemQuantity("mario.rossi@example.com", "PRD-001", 3).Returns(true);

        // Act
        var risultato = sut.IncrementCartItem("PRD-001");

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Cart");
        dataService.Received(1).UpdateCartItemQuantity("mario.rossi@example.com", "PRD-001", 3);
    }

    [Fact]
    public void DecrementCartItem_QuandoQuantitaUno_AlloraRimuoveArticolo()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.GetCart("mario.rossi@example.com").Returns(new CartViewModel
        {
            Items = [new CartItemViewModel { ProductCode = "PRD-001", Quantity = 1, Stock = 5 }]
        });
        dataService.RemoveCartItem("mario.rossi@example.com", "PRD-001").Returns(true);

        // Act
        var risultato = sut.DecrementCartItem("PRD-001");

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Cart");
        dataService.Received(1).RemoveCartItem("mario.rossi@example.com", "PRD-001");
        dataService.DidNotReceive().UpdateCartItemQuantity(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public void CheckoutCart_QuandoCarrelloValido_AlloraAvviaCheckoutSenzaCreareOrdine()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.StartCheckout("mario.rossi@example.com").Returns(true);

        // Act
        var risultato = sut.CheckoutCart();

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("CheckoutSummary");
        dataService.Received(1).StartCheckout("mario.rossi@example.com");
        dataService.DidNotReceive().CreateOrder(Arg.Any<string?>(), Arg.Any<IReadOnlyList<NewOrderItemViewModel>>());
        dataService.DidNotReceive().ClearCart(Arg.Any<string?>());
    }

    [Fact]
    public void CheckoutSummary_QuandoSessionePresente_AlloraMostraRiepilogo()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        var checkout = new CheckoutSessionViewModel
        {
            Cart = new CartViewModel(),
            TotalItems = 1,
            TotalAmount = 25m
        };
        dataService.GetCheckout("mario.rossi@example.com").Returns(checkout);

        // Act
        var risultato = sut.CheckoutSummary();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(checkout);
    }

    [Fact]
    public void ItalianProvinces_QuandoRichiesto_AlloraRestituisceProvinceDalServizio()
    {
        // Arrange
        dataService.GetItalianProvinces().Returns(["Ancona", "Pesaro e Urbino"]);

        // Act
        var risultato = sut.ItalianProvinces();

        // Assert
        var json = risultato.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().BeEquivalentTo(new[] { "Ancona", "Pesaro e Urbino" }, options => options.WithStrictOrdering());
        dataService.Received(1).GetItalianProvinces();
    }

    [Fact]
    public void ItalianCities_QuandoProvinciaValida_AlloraRestituisceCittaDalServizio()
    {
        // Arrange
        dataService.GetItalianCities("Pesaro e Urbino").Returns(["Fano", "Pesaro"]);

        // Act
        var risultato = sut.ItalianCities("Pesaro e Urbino");

        // Assert
        var json = risultato.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().BeEquivalentTo(new[] { "Fano", "Pesaro" }, options => options.WithStrictOrdering());
        dataService.Received(1).GetItalianCities("Pesaro e Urbino");
    }

    [Fact]
    public void ItalianCities_QuandoProvinciaVuota_AlloraRestituisceListaVuota()
    {
        // Act
        var risultato = sut.ItalianCities(string.Empty);

        // Assert
        var json = risultato.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().BeEquivalentTo(Array.Empty<string>());
        dataService.DidNotReceive().GetItalianCities(Arg.Any<string>());
    }

    [Fact]
    public void ItalianPostalCodes_QuandoCittaValida_AlloraRestituisceCapDalServizio()
    {
        // Arrange
        dataService.GetItalianPostalCodes("Pesaro e Urbino", "Pesaro").Returns(["61121", "61122"]);

        // Act
        var risultato = sut.ItalianPostalCodes("Pesaro e Urbino", "Pesaro");

        // Assert
        var json = risultato.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().BeEquivalentTo(new[] { "61121", "61122" }, options => options.WithStrictOrdering());
        dataService.Received(1).GetItalianPostalCodes("Pesaro e Urbino", "Pesaro");
    }

    [Theory]
    [InlineData("", "Pesaro")]
    [InlineData("Pesaro e Urbino", "")]
    public void ItalianPostalCodes_QuandoInputVuoto_AlloraRestituisceListaVuota(string provinceName, string cityName)
    {
        // Act
        var risultato = sut.ItalianPostalCodes(provinceName, cityName);

        // Assert
        var json = risultato.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().BeEquivalentTo(Array.Empty<string>());
        dataService.DidNotReceive().GetItalianPostalCodes(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void CheckoutConfirm_Post_QuandoPagamentoRichiesto_AlloraReindirizzaAPayment()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        dataService.ConfirmCheckout("mario.rossi@example.com").Returns(new CheckoutConfirmResult
        {
            Success = true,
            OrderId = 42,
            RequiresPayment = true
        });

        // Act
        var risultato = sut.CheckoutConfirmPost();

        // Assert
        var redirect = risultato.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("CheckoutPayment");
        redirect.RouteValues.Should().ContainKey("orderId").WhoseValue.Should().Be(42);
    }

    [Fact]
    public void OrderDetails_QuandoClienteProprietario_AlloraMostraDettaglio()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        var dettaglio = new OrderDetailsViewModel { Order = new Order { Id = 42 } };
        dataService.GetOrderDetails(42, "mario.rossi@example.com", false).Returns(dettaglio);

        // Act
        var risultato = sut.OrderDetails(42);

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dettaglio);
    }

    [Fact]
    public void OrderDetails_QuandoServizioNonRestituisceOrdine_AlloraNotFound()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");

        // Act
        var risultato = sut.OrderDetails(42);

        // Assert
        risultato.Should().BeOfType<NotFoundResult>();
    }

    private static ClaimsPrincipal CreateUser(string email, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            authenticationType: "Test"));
    }
}
