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
        dbContext.Orders.Include(order => order.Items).Include(order => order.StatusHistory).Single().Should().BeEquivalentTo(new
        {
            TotalAmount = 50m,
            Status = (int)OrderStatus.Pending
        });
        dbContext.OrderStatusHistory.Should().ContainSingle(history =>
            history.FromStatus == null &&
            history.ToStatus == (int)OrderStatus.Pending &&
            history.ChangedBy == "nuovo.utente@example.com" &&
            history.Reason == "Order created");
        paginaOrdini.Orders.Should().ContainSingle(order =>
            order.Customer.Email == "nuovo.utente@example.com" &&
            order.TotalAmount == 50m &&
            order.Items.Single().ProductName == "Laptop tracer" &&
            order.Items.Single().Quantity == 2);
    }

    [Fact]
    public void CreateOrder_QuandoOrdineContienePiuProdotti_AlloraCreaRigheRiduceStockETotaleOrdine()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        SeedProduct(dbContext, code: "PRD-002", name: "Cuffie tracer", stockQuantity: 4, price: 15m);
        var sut = new DashboardOrdersDataService(dbContext);
        var items = new List<NewOrderItemViewModel>
        {
            new() { ProductCode = "prd-001", Quantity = 2 },
            new() { ProductCode = "prd-002", Quantity = 3 }
        };

        // Act
        var risultato = sut.CreateOrder("nuovo.utente@example.com", items);
        var ordine = dbContext.Orders.Include(order => order.Items).Include(order => order.StatusHistory).Single();
        var paginaOrdini = sut.GetOrdersPageDataForCustomerEmail("nuovo.utente@example.com", pageSize: 0);

        // Assert
        risultato.Should().BeTrue();
        ordine.TotalAmount.Should().Be(95m);
        ordine.Items.Should().HaveCount(2);
        ordine.StatusHistory.Should().ContainSingle(history =>
            history.FromStatus == null &&
            history.ToStatus == (int)OrderStatus.Pending &&
            history.ChangedBy == "nuovo.utente@example.com");
        ordine.Items.Should().Contain(item => item.ProductId == dbContext.Products.Single(product => product.Code == "PRD-001").Id && item.Quantity == 2 && item.UnitPrice == 25m);
        ordine.Items.Should().Contain(item => item.ProductId == dbContext.Products.Single(product => product.Code == "PRD-002").Id && item.Quantity == 3 && item.UnitPrice == 15m);
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(3);
        dbContext.Products.Single(product => product.Code == "PRD-002").StockQuantity.Should().Be(1);
        paginaOrdini.Orders.Should().ContainSingle(order =>
            order.Customer.Email == "nuovo.utente@example.com" &&
            order.TotalAmount == 95m &&
            order.Items.Count == 2 &&
            order.Items.Any(item => item.ProductName == "Laptop tracer" && item.Quantity == 2) &&
            order.Items.Any(item => item.ProductName == "Cuffie tracer" && item.Quantity == 3));
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
    public void CreateOrder_QuandoOrdineContieneProdottiDuplicati_AlloraNonCreaOrdine()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        var items = new List<NewOrderItemViewModel>
        {
            new() { ProductCode = "PRD-001", Quantity = 1 },
            new() { ProductCode = "prd-001", Quantity = 2 }
        };

        // Act
        var risultato = sut.CreateOrder("nuovo.utente@example.com", items);

        // Assert
        risultato.Should().BeFalse();
        dbContext.Orders.Should().BeEmpty();
        dbContext.Customers.Should().BeEmpty();
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(5);
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
    public void GetOrders_QuandoIntervalloDateValido_AlloraRestituisceSoloOrdiniInclusi()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext, orderNumber: "ORD-001", createdAt: new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc));
        SeedOrder(dbContext, orderNumber: "ORD-002", createdAt: new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc));
        SeedOrder(dbContext, orderNumber: "ORD-003", createdAt: new DateTime(2026, 4, 20, 18, 0, 0, DateTimeKind.Utc));
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetOrders(pageSize: 0, dateFrom: "2026-04-10", dateTo: "2026-04-20");

        // Assert
        risultato.Items.Should().HaveCount(2);
        risultato.Items.Should().OnlyContain(order =>
            order.OrderDate.Date >= new DateTime(2026, 4, 10) &&
            order.OrderDate.Date <= new DateTime(2026, 4, 20));
        risultato.Items.Select(order => order.OrderNumber).Should().BeEquivalentTo(["ORD-002", "ORD-003"]);
    }

    [Fact]
    public void GetOrdersPageData_QuandoDateNonValide_AlloraNormalizzaFiltriDataVuoti()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedOrder(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetOrdersPageData(pageSize: 0, dateFrom: "non-valida", dateTo: "non-valida");

        // Assert
        risultato.DateFrom.Should().BeEmpty();
        risultato.DateTo.Should().BeEmpty();
        risultato.TotalOrders.Should().Be(1);
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

    [Fact]
    public void GetProducts_QuandoProdottoHaImmagine_AlloraMappaImageUrl()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetProducts(page: 1, pageSize: 10);

        // Assert
        risultato.Items.Single().ImageUrl.Should().Be("https://loremflickr.com/320/240/laptop,computer/all?lock=1");
    }

    [Fact]
    public void CreateProduct_QuandoImmagineValida_AlloraSalvaImageUrl()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedCategory(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);
        var product = new Product
        {
            Code = "prd-002",
            Name = "Cuffie Flex",
            Category = new Category { Code = "CAT-001" },
            Description = "Cuffie per ufficio",
            ImageUrl = "https://loremflickr.com/320/240/headphones,audio/all?lock=2",
            UnitCost = 49m,
            Stock = 7
        };

        // Act
        var risultato = sut.CreateProduct(product);

        // Assert
        risultato.Should().BeTrue();
        dbContext.Products.Single(product => product.Code == "PRD-002").ImageUrl.Should().Be("https://loremflickr.com/320/240/headphones,audio/all?lock=2");
    }

    [Fact]
    public void UpdateProduct_QuandoImmagineValida_AlloraAggiornaImageUrl()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        var product = new Product
        {
            Code = "PRD-001",
            Name = "Laptop tracer",
            Category = new Category { Code = "CAT-001" },
            Description = "Prodotto aggiornato",
            ImageUrl = "https://loremflickr.com/320/240/computer,office/all?lock=99",
            UnitCost = 30m,
            Stock = 4
        };

        // Act
        var risultato = sut.UpdateProduct(product);

        // Assert
        risultato.Should().BeTrue();
        dbContext.Products.Single(product => product.Code == "PRD-001").ImageUrl.Should().Be("https://loremflickr.com/320/240/computer,office/all?lock=99");
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
            Description = "Prodotto per tracer bullet",
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

    private static void SeedOrder(
        DashboardOrdersDbContext dbContext,
        string orderNumber = "ORD-001",
        DateTime? createdAt = null)
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
            OrderNumber = orderNumber,
            Customer = customer,
            TotalAmount = product.Price,
            Status = (int)OrderStatus.Pending,
            CreatedAt = createdAt ?? DateTime.UtcNow,
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
