using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DashboardOrders.Tests;

public class DashboardOrdersDataServiceTests
{
    [Fact]
    public void CreateOrder_QuandoUtenteNuovoEProdottoDisponibile_AlloraCreaOrdineRiduceStockEOrdiniUtenteVisibili()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.CreateOrder("nuovo.utente@example.com", "prd-001", 2);
        var paginaOrdini = sut.GetOrdersPageDataForCustomerEmail("nuovo.utente@example.com", pageSize: 0);

        // Assert
        risultato.Should().BeTrue();
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(3);
        dbContext.Customers.Single().Email.Should().Be("nuovo.utente@example.com");
        dbContext.Orders.Include(order => order.Items).Single().Should().BeEquivalentTo(new
        {
            TotalAmount = 50m,
            Status = (int)OrderStatus.Pending
        });
        paginaOrdini.Orders.Should().ContainSingle(order =>
            order.Customer.Email == "nuovo.utente@example.com" &&
            order.TotalAmount == 50m &&
            order.Items.Single().ProductName == "Laptop tracer" &&
            order.Items.Single().Quantity == 2);
    }

    [Fact]
    public void CreateOrder_QuandoStockInsufficiente_AlloraNonCreaOrdineENonRiduceStock()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 1, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.CreateOrder("nuovo.utente@example.com", "PRD-001", 2);

        // Assert
        risultato.Should().BeFalse();
        dbContext.Orders.Should().BeEmpty();
        dbContext.Customers.Should().BeEmpty();
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(1);
    }

    [Fact]
    public void GetOrders_QuandoPageNumeroNegativo_AlloraNormalizzaAPrimaPagina()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetOrders(page: -1, pageSize: 10);

        // Assert
        risultato.CurrentPage.Should().Be(1);
        risultato.Items.Should().NotBeEmpty();
    }

    [Fact]
    public void GetOrders_QuandoPageSuperioreAlTotale_AlloraUltimaPagina()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetOrders(page: 99, pageSize: 10);

        // Assert
        risultato.CurrentPage.Should().Be(1); // Only one page available
        risultato.TotalPages.Should().Be(1);
    }

    [Fact]
    public void GetOrders_QuandoPageSizeZero_AlloraMostraTutti()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetOrders(page: 1, pageSize: 0);

        // Assert
        risultato.PageSize.Should().Be(0);
        risultato.Items.Should().HaveCount(1); // All items
    }

    [Fact]
    public void GetCustomers_QuandoNessunCliente_AlloraPaginaVuota()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetCustomers(page: 1, pageSize: 10);

        // Assert
        risultato.Items.Should().BeEmpty();
        risultato.TotalPages.Should().Be(1);
        risultato.CurrentPage.Should().Be(1);
    }

    [Fact]
    public void GetProducts_QuandoCategoriaInesistente_AlloraNessunProdotto()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetProducts(page: 1, pageSize: 10, categoryCode: "CAT-999");

        // Assert
        risultato.Items.Should().BeEmpty();
        risultato.TotalPages.Should().Be(1);
    }

    [Fact]
    public void GetOrders_QuandoFiltroClienteVuoto_AlloraTuttiGliOrdini()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultatoSenzaFiltro = sut.GetOrders(customerId: null, pageSize: 0);
        var risultatoConFiltro = sut.GetOrders(customerId: 999, pageSize: 0);

        // Assert
        risultatoSenzaFiltro.Items.Should().NotBeEmpty();
        risultatoConFiltro.Items.Should().BeEmpty();
    }

    [Fact]
    public void GetProducts_QuandoRicercaBreve_AlloraNonFiltra()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetProducts(page: 1, pageSize: 10, search: "a"); // Less than 3 chars

        // Assert
        risultato.Items.Should().NotBeEmpty(); // Should return all products
    }

    private static DashboardOrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DashboardOrdersDbContext(options);
    }

    private static void SeedProduct(DashboardOrdersDbContext dbContext, int stockQuantity, decimal price)
    {
        dbContext.Categories.Add(new CategoryEntity
        {
            Code = "CAT-001",
            Name = "Informatica",
            Description = "Prodotti informatici"
        });
        dbContext.Products.Add(new ProductEntity
        {
            Code = "PRD-001",
            Name = "Laptop tracer",
            Description = "Prodotto per tracer bullet",
            Price = price,
            StockQuantity = stockQuantity,
            CategoryCode = "CAT-001",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }

    private static void SeedOrder(DashboardOrdersDbContext dbContext)
    {
        var product = dbContext.Products.Single(product => product.Code == "PRD-001");
        var customer = new CustomerEntity
        {
            Name = "Mario Rossi",
            Email = "mario.rossi@example.com",
            Phone = "+390212345678",
            AvatarInitials = "MR",
            CreatedAt = DateTime.UtcNow
        };
        var order = new OrderEntity
        {
            OrderNumber = "ORD-001",
            Customer = customer,
            TotalAmount = product.Price,
            Status = (int)OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items =
            [
                new OrderItemEntity
                {
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                }
            ]
        };

        dbContext.Customers.Add(customer);
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();
    }
}
