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
}
