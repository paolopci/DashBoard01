using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DashboardOrders.Tests;

public class ShoppingCartDataServiceTests
{
    [Fact]
    public void AddOrUpdateCartItem_QuandoProdottoDisponibile_AlloraCreaCarrelloPersistente()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        var before = DateTime.UtcNow;

        // Act
        var risultato = sut.AddOrUpdateCartItem("Cliente@Test.IT", "prd-001", 2);
        var carrello = sut.GetCart("cliente@test.it");

        // Assert
        risultato.Should().BeTrue();
        carrello.TotalItems.Should().Be(2);
        carrello.TotalAmount.Should().Be(50m);
        carrello.Items.Should().ContainSingle(item =>
            item.ProductCode == "PRD-001" &&
            item.ProductName == "Laptop tracer" &&
            item.Quantity == 2 &&
            item.UnitPrice == 25m &&
            item.Stock == 5);
        carrello.ExpiresAt.Should().NotBeNull();
        carrello.ExpiresAt.Should().BeAfter(before.AddDays(29));
        dbContext.Carts.Include(cart => cart.Items).Should().ContainSingle(cart =>
            cart.CustomerEmail == "cliente@test.it" &&
            cart.Items.Count == 1);
    }

    [Fact]
    public void AddOrUpdateCartItem_QuandoProdottoGiaPresente_AlloraIncrementaQuantitaSenzaDuplicati()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();

        // Act
        var risultato = sut.AddOrUpdateCartItem("cliente@test.it", "prd-001", 3);
        var carrello = sut.GetCart("cliente@test.it");

        // Assert
        risultato.Should().BeTrue();
        carrello.Items.Should().ContainSingle(item => item.ProductCode == "PRD-001" && item.Quantity == 5);
        dbContext.CartItems.Should().ContainSingle();
    }

    [Fact]
    public void AddOrUpdateCartItem_QuandoQuantitaSuperaStock_AlloraNonAggiornaCarrello()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 2, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();

        // Act
        var risultato = sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2);

        // Assert
        risultato.Should().BeFalse();
        sut.GetCart("cliente@test.it").Items.Single().Quantity.Should().Be(1);
    }

    [Fact]
    public void UpdateCartItemQuantity_QuandoQuantitaValida_AlloraAggiornaQuantita()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();

        // Act
        var risultato = sut.UpdateCartItemQuantity("cliente@test.it", "prd-001", 4);

        // Assert
        risultato.Should().BeTrue();
        sut.GetCart("cliente@test.it").Items.Single().Quantity.Should().Be(4);
    }

    [Fact]
    public void UpdateCartItemQuantity_QuandoQuantitaNonValida_AlloraNonAggiorna()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();

        // Act
        var risultatoZero = sut.UpdateCartItemQuantity("cliente@test.it", "PRD-001", 0);
        var risultatoStock = sut.UpdateCartItemQuantity("cliente@test.it", "PRD-001", 6);

        // Assert
        risultatoZero.Should().BeFalse();
        risultatoStock.Should().BeFalse();
        sut.GetCart("cliente@test.it").Items.Single().Quantity.Should().Be(2);
    }

    [Fact]
    public void RemoveCartItem_QuandoArticoloPresente_AlloraRimuoveSoloQuellArticolo()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        SeedProduct(dbContext, code: "PRD-002", name: "Cuffie tracer", stockQuantity: 4, price: 15m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-002", 1).Should().BeTrue();

        // Act
        var risultato = sut.RemoveCartItem("cliente@test.it", "prd-001");

        // Assert
        risultato.Should().BeTrue();
        sut.GetCart("cliente@test.it").Items.Should().ContainSingle(item =>
            item.ProductCode == "PRD-002" &&
            item.Quantity == 1);
    }

    [Fact]
    public void ClearCart_QuandoCarrelloPresente_AlloraSvuotaArticoli()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();

        // Act
        var risultato = sut.ClearCart("cliente@test.it");

        // Assert
        risultato.Should().BeTrue();
        sut.GetCart("cliente@test.it").Items.Should().BeEmpty();
        dbContext.CartItems.Should().BeEmpty();
    }

    [Fact]
    public void GetCart_QuandoCarrelloScaduto_AlloraLoCancellaERestituisceVuoto()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var product = dbContext.Products.Single(product => product.Code == "PRD-001");
        dbContext.Carts.Add(new CartEntity
        {
            CustomerEmail = "cliente@test.it",
            CreatedAt = DateTime.UtcNow.AddDays(-40),
            UpdatedAt = DateTime.UtcNow.AddDays(-40),
            ExpiresAt = DateTime.UtcNow.AddDays(-10),
            Items =
            [
                new CartItemEntity
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow.AddDays(-40),
                    UpdatedAt = DateTime.UtcNow.AddDays(-40)
                }
            ]
        });
        dbContext.SaveChanges();
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var carrello = sut.GetCart("cliente@test.it");

        // Assert
        carrello.Items.Should().BeEmpty();
        dbContext.Carts.Should().BeEmpty();
        dbContext.CartItems.Should().BeEmpty();
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
        SeedProduct(dbContext, "PRD-001", "Laptop tracer", stockQuantity, price);
    }

    private static void SeedProduct(DashboardOrdersDbContext dbContext, string code, string name, int stockQuantity, decimal price)
    {
        SeedCategory(dbContext);
        dbContext.Products.Add(new ProductEntity
        {
            Code = code,
            Name = name,
            Description = "Prodotto per carrello",
            Price = price,
            StockQuantity = stockQuantity,
            CategoryCode = "CAT-001",
            ImageUrl = "https://loremflickr.com/320/240/laptop,computer/all?lock=1",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }

    private static void SeedCategory(DashboardOrdersDbContext dbContext)
    {
        if (dbContext.Categories.Any(category => category.Code == "CAT-001"))
        {
            return;
        }

        dbContext.Categories.Add(new CategoryEntity
        {
            Code = "CAT-001",
            Name = "Informatica",
            Description = "Prodotti informatici"
        });
        dbContext.SaveChanges();
    }
}
