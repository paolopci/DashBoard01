using DashboardOrders.Controllers;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using Xunit;

namespace DashboardOrders.Tests;

public class HomeControllerTests
{
    private readonly IDashboardOrdersDataService dataService;
    private readonly HomeController sut;

    public HomeControllerTests()
    {
        dataService = Substitute.For<IDashboardOrdersDataService>();
        sut = new HomeController(dataService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser("admin@micene.it")
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
    }

    [Fact]
    public void NewOrder_Post_QuandoUtenteNonAdminEOrdineValido_AlloraCreaOrdineEReindirizzaAOrders()
    {
        // Arrange
        sut.ControllerContext.HttpContext.User = CreateUser("mario.rossi@example.com");
        var model = new NewOrderViewModel { ProductCode = "PRD-001", Quantity = 2 };
        dataService.CreateOrder("mario.rossi@example.com", "PRD-001", 2).Returns(true);

        // Act
        var risultato = sut.NewOrder(model);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Orders");
        dataService.Received(1).CreateOrder("mario.rossi@example.com", "PRD-001", 2);
    }

    private static ClaimsPrincipal CreateUser(string email)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, email)],
            authenticationType: "Test"));
    }
}
