using DashboardOrders.Controllers;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
        sut = new HomeController(dataService);
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
}
