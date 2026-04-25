using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Xunit;

namespace DashboardOrders.Tests;

public class MockDataServiceTests
{
    [Fact]
    public void GetDashboardData_QuandoRichiestaPrimaPagina_AlloraRestituisceOrdiniOrdinatiPerDataDecrescente()
    {
        // Arrange
        const int pageSize = 10;

        // Act
        var result = MockDataService.GetDashboardData(page: 1, pageSize: pageSize, sortBy: "date", sortDirection: "desc");

        // Assert
        result.RecentOrders.Should().HaveCount(pageSize).And.BeInDescendingOrder(order => order.OrderDate);
    }

    [Fact]
    public void GetDashboardData_QuandoPageSizeNonValido_AlloraMostraTuttiGliOrdini()
    {
        // Arrange
        var totalOrders = MockDataService.GetOrders().Count;

        // Act
        var result = MockDataService.GetDashboardData(pageSize: 999);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            PageSize = 0,
            CurrentPage = 1,
            TotalPages = 1,
            TotalOrders = totalOrders
        });
    }

    [Fact]
    public void GetDashboardData_QuandoSortNonValido_AlloraUsaOrdinamentoPredefinito()
    {
        // Arrange
        const string sortBy = "non-valido";
        const string sortDirection = "non-valido";

        // Act
        var result = MockDataService.GetDashboardData(sortBy: sortBy, sortDirection: sortDirection);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            SortBy = "date",
            SortDirection = "desc"
        });
    }

    [Fact]
    public void GetDashboardData_QuandoRicercaValida_AlloraRestituisceSoloOrdiniCoerenti()
    {
        // Arrange
        const string search = "ORD-2026-001";

        // Act
        var result = MockDataService.GetDashboardData(pageSize: 0, search: search);

        // Assert
        result.SearchTerm.Should().Be(search);
        result.RecentOrders.Should().OnlyContain(order =>
            order.OrderNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            order.Product.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            OrderStatusPresentation.FromStatus(order.Status).Label.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetDashboardData_QuandoInvocato_AlloraPendingOrdersERicaviUsanoClassificazioneCentrale()
    {
        // Arrange
        var ordini = MockDataService.GetOrders();
        var ricavoAtteso = ordini
            .Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(order.Status))
            .Sum(order => order.TotalAmount);
        var attiviAttesi = ordini.Count(order => OrderStatusMetricsPolicy.IsOperationallyActive(order.Status));

        // Act
        var result = MockDataService.GetDashboardData(pageSize: 0);

        // Assert
        result.TotalRevenue.Should().Be(ricavoAtteso);
        result.PendingOrders.Should().Be(attiviAttesi);
    }

    [Fact]
    public void GetOrdersPageData_QuandoClienteValido_AlloraRestituisceSoloOrdiniDelCliente()
    {
        // Arrange
        var customerId = MockDataService.GetOrders().First().Customer.Id;

        // Act
        var result = MockDataService.GetOrdersPageData(customerId, pageSize: 0);

        // Assert
        result.Orders.Should().OnlyContain(order => order.Customer.Id == customerId);
    }

    [Fact]
    public void GetOrdersPageData_QuandoSortNull_AlloraUsaOrdinamentoPredefinito()
    {
        // Arrange
        string sortBy = null!;
        string sortDirection = null!;

        // Act
        var result = MockDataService.GetOrdersPageData(sortBy: sortBy, sortDirection: sortDirection);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            SortBy = "date",
            SortDirection = "desc"
        });
    }

    [Fact]
    public void GetOrdersPageData_QuandoRicercaValida_AlloraRestituisceSoloOrdiniCoerenti()
    {
        // Arrange
        const string search = "Laptop";

        // Act
        var result = MockDataService.GetOrdersPageData(pageSize: 0, search: search);

        // Assert
        result.SearchTerm.Should().Be(search);
        result.Orders.Should().OnlyContain(order =>
            order.OrderNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            order.Items.Any(item => item.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
            OrderStatusPresentation.FromStatus(order.Status).Label.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetOrdersPageData_QuandoRicercaNull_AlloraNormalizzaTermineVuoto()
    {
        // Arrange
        string search = null!;

        // Act
        var result = MockDataService.GetOrdersPageData(search: search);

        // Assert
        result.SearchTerm.Should().BeEmpty();
    }

    [Fact]
    public void GetOrdersPageData_QuandoIntervalloDateValido_AlloraRestituisceSoloOrdiniInclusi()
    {
        // Arrange
        var referenceDate = MockDataService.GetOrders().OrderBy(order => order.OrderDate).Skip(5).First().OrderDate.Date;
        var dateFrom = referenceDate.ToString("yyyy-MM-dd");
        var dateTo = referenceDate.AddDays(10).ToString("yyyy-MM-dd");

        // Act
        var result = MockDataService.GetOrdersPageData(pageSize: 0, dateFrom: dateFrom, dateTo: dateTo);

        // Assert
        result.DateFrom.Should().Be(dateFrom);
        result.DateTo.Should().Be(dateTo);
        result.Orders.Should().OnlyContain(order =>
            order.OrderDate.Date >= referenceDate &&
            order.OrderDate.Date <= referenceDate.AddDays(10));
    }

    [Fact]
    public void GetOrdersPageData_QuandoDateNonValide_AlloraNonApplicaFiltroData()
    {
        // Arrange
        var totalOrders = MockDataService.GetOrders().Count;

        // Act
        var result = MockDataService.GetOrdersPageData(pageSize: 0, dateFrom: "non-valida", dateTo: "non-valida");

        // Assert
        result.DateFrom.Should().BeEmpty();
        result.DateTo.Should().BeEmpty();
        result.TotalOrders.Should().Be(totalOrders);
    }

    [Fact]
    public void GetOrdersPageData_QuandoInvocato_AlloraShippedOrdersUsaClassificazioneCentrale()
    {
        // Arrange
        var ordini = MockDataService.GetOrders();
        var evasiAttesi = ordini.Count(order => OrderStatusMetricsPolicy.IsFulfillmentCompleted(order.Status));

        // Act
        var result = MockDataService.GetOrdersPageData(pageSize: 0);

        // Assert
        result.ShippedOrders.Should().Be(evasiAttesi);
    }

    [Fact]
    public void GetCustomersPageData_QuandoRicercaValida_AlloraRestituisceSoloClientiCoerenti()
    {
        // Arrange
        const string search = "marco";

        // Act
        var result = MockDataService.GetCustomersPageData(pageSize: 0, search: search);

        // Assert
        result.Customers.Should().OnlyContain(summary =>
            summary.Customer.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            summary.Customer.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetCustomersPageData_QuandoRicercaNull_AlloraNormalizzaTermineVuoto()
    {
        // Arrange
        string search = null!;

        // Act
        var result = MockDataService.GetCustomersPageData(search: search);

        // Assert
        result.SearchTerm.Should().BeEmpty();
    }

    [Fact]
    public void GetCustomersPageData_QuandoSortNonValido_AlloraUsaOrdinamentoPredefinito()
    {
        // Arrange
        const string sortBy = "non-valido";
        const string sortDirection = "non-valido";

        // Act
        var result = MockDataService.GetCustomersPageData(sortBy: sortBy, sortDirection: sortDirection);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            SortBy = "totalAmount",
            SortDirection = "desc"
        });
    }

    [Fact]
    public void GetProductsPageData_QuandoCategoriaValida_AlloraRestituisceSoloProdottiDellaCategoria()
    {
        // Arrange
        var categoryCode = MockDataService.GetCategories().First().Code;

        // Act
        var result = MockDataService.GetProductsPageData(pageSize: 0, categoryCode: categoryCode);

        // Assert
        result.Products.Should().OnlyContain(product => product.Category.Code == categoryCode);
    }

    [Fact]
    public void GetProductsPageData_QuandoCategoriaNull_AlloraNonApplicaFiltroCategoria()
    {
        // Arrange
        string categoryCode = null!;
        var totalProducts = MockDataService.GetProducts().Count;

        // Act
        var result = MockDataService.GetProductsPageData(pageSize: 0, categoryCode: categoryCode);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            SelectedCategoryCode = string.Empty,
            TotalProducts = totalProducts
        });
    }

    [Fact]
    public void GetProductsPageData_QuandoRicercaValida_AlloraRestituisceSoloProdottiCoerenti()
    {
        // Arrange
        const string search = "Laptop";

        // Act
        var result = MockDataService.GetProductsPageData(pageSize: 0, search: search);

        // Assert
        result.SearchTerm.Should().Be(search);
        result.Products.Should().OnlyContain(product =>
            product.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            product.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            product.Category.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            product.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            product.Category.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetProductsPageData_QuandoRicercaNull_AlloraNormalizzaTermineVuoto()
    {
        // Arrange
        string search = null!;

        // Act
        var result = MockDataService.GetProductsPageData(search: search);

        // Assert
        result.SearchTerm.Should().BeEmpty();
    }

    [Fact]
    public void GetProductsPageData_QuandoSortNonValido_AlloraUsaOrdinamentoPredefinito()
    {
        // Arrange
        const string sortBy = "non-valido";
        const string sortDirection = "non-valido";

        // Act
        var result = MockDataService.GetProductsPageData(sortBy: sortBy, sortDirection: sortDirection);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            SortBy = "name",
            SortDirection = "asc"
        });
    }

    [Fact]
    public void GetCategoryPageData_QuandoOrdinamentoPerNomeDesc_AlloraOrdinaCategoriePerNomeDecrescente()
    {
        // Arrange
        const string sortBy = "name";
        const string sortDirection = "desc";

        // Act
        var result = MockDataService.GetCategoryPageData(sortBy, sortDirection);

        // Assert
        result.Categories.Should().BeInDescendingOrder(category => category.Name);
    }

    [Fact]
    public void GetCategoryPageData_QuandoSortNonValido_AlloraUsaOrdinamentoPredefinito()
    {
        // Arrange
        const string sortBy = "non-valido";
        const string sortDirection = "non-valido";

        // Act
        var result = MockDataService.GetCategoryPageData(sortBy, sortDirection);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            SortBy = "code",
            SortDirection = "asc"
        });
    }
}
