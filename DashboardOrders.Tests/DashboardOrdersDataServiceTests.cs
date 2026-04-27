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
    public void ChangeOrderStatus_QuandoTransizioneValida_AlloraAggiornaStatoEStorico()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var orderId = SeedOrder(dbContext);
        SeedInitialStatusHistory(dbContext, orderId, OrderStatus.Pending);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.PaymentPending, "admin@example.com", "Avvio pagamento", "corr-001");

        // Assert
        risultato.Should().BeTrue();
        var ordine = dbContext.Orders.Include(order => order.StatusHistory).Single(order => order.Id == orderId);
        ordine.Status.Should().Be((int)OrderStatus.PaymentPending);
        ordine.UpdatedAt.Should().NotBeNull();
        ordine.StatusHistory.Should().Contain(history =>
            history.FromStatus == (int)OrderStatus.Pending &&
            history.ToStatus == (int)OrderStatus.PaymentPending &&
            history.ChangedBy == "admin@example.com" &&
            history.Reason == "Avvio pagamento" &&
            history.CorrelationId == "corr-001");
    }

    [Fact]
    public void ChangeOrderStatus_QuandoTransizioneNonValida_AlloraNonAggiornaStato()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var orderId = SeedOrder(dbContext);
        SeedInitialStatusHistory(dbContext, orderId, OrderStatus.Pending);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.Delivered, "admin@example.com", "Salto non ammesso");

        // Assert
        risultato.Should().BeFalse();
        var ordine = dbContext.Orders.Include(order => order.StatusHistory).Single(order => order.Id == orderId);
        ordine.Status.Should().Be((int)OrderStatus.Pending);
        ordine.StatusHistory.Should().HaveCount(1);
    }

    [Fact]
    public void ChangeOrderStatus_QuandoAnnullaDaStatoPreFulfillment_AlloraRipristinaStockEAggiungeStorico()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.CreateOrder("nuovo.utente@example.com", "PRD-001", 2).Should().BeTrue();
        var orderId = dbContext.Orders.Single().Id;
        var stockPrima = dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity;

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.Cancelled, "admin@example.com", "Ordine annullato");

        // Assert
        risultato.Should().BeTrue();
        stockPrima.Should().Be(3);
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(5);
        dbContext.OrderStatusHistory.Should().Contain(history =>
            history.FromStatus == (int)OrderStatus.Pending &&
            history.ToStatus == (int)OrderStatus.Cancelled &&
            history.Reason == "Ordine annullato");
    }

    [Fact]
    public void ChangeOrderStatus_QuandoPaymentFailedDaPaymentPending_AlloraRipristinaStock()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.CreateOrder("nuovo.utente@example.com", "PRD-001", 2).Should().BeTrue();
        var orderId = dbContext.Orders.Single().Id;
        sut.ChangeOrderStatus(orderId, OrderStatus.PaymentPending, "admin@example.com", "Pagamento avviato").Should().BeTrue();

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.PaymentFailed, "admin@example.com", "Pagamento rifiutato");

        // Assert
        risultato.Should().BeTrue();
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(5);
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.PaymentFailed);
    }

    [Fact]
    public void ChangeOrderStatus_QuandoPaymentFailedGiaRegistrato_AlloraNonRipristinaStockDueVolte()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.CreateOrder("nuovo.utente@example.com", "PRD-001", 2).Should().BeTrue();
        var orderId = dbContext.Orders.Single().Id;
        sut.ChangeOrderStatus(orderId, OrderStatus.PaymentPending, "admin@example.com", "Pagamento avviato").Should().BeTrue();
        sut.ChangeOrderStatus(orderId, OrderStatus.PaymentFailed, "admin@example.com", "Pagamento rifiutato").Should().BeTrue();
        sut.ChangeOrderStatus(orderId, OrderStatus.PaymentPending, "admin@example.com", "Retry pagamento").Should().BeTrue();

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.PaymentFailed, "admin@example.com", "Secondo rifiuto");

        // Assert
        risultato.Should().BeFalse();
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(5);
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.PaymentPending);
    }

    [Fact]
    public void ChangeOrderStatus_QuandoStatoRichiedeReasonEMotivoManca_AlloraRestituisceFalse()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.CreateOrder("nuovo.utente@example.com", "PRD-001", 1).Should().BeTrue();
        var orderId = dbContext.Orders.Single().Id;

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.Cancelled, "admin@example.com", string.Empty);

        // Assert
        risultato.Should().BeFalse();
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.Pending);
        dbContext.OrderStatusHistory.Should().HaveCount(1);
    }

    [Fact]
    public void ChangeOrderStatus_QuandoOrdineNonEsiste_AlloraRestituisceFalse()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.ChangeOrderStatus(999, OrderStatus.PaymentPending, "admin@example.com", "Tentativo");

        // Assert
        risultato.Should().BeFalse();
    }

    [Fact]
    public void ChangeOrderStatus_QuandoOrdineInStatoTerminale_AlloraNonAggiornaStato()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var orderId = SeedOrder(dbContext);
        var ordine = dbContext.Orders.Single(order => order.Id == orderId);
        ordine.Status = (int)OrderStatus.Cancelled;
        SeedInitialStatusHistory(dbContext, orderId, OrderStatus.Cancelled);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.ChangeOrderStatus(orderId, OrderStatus.PaymentPending, "admin@example.com", "Riapertura");

        // Assert
        risultato.Should().BeFalse();
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.Cancelled);
        dbContext.OrderStatusHistory.Should().HaveCount(1);
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
    public void GetOrders_QuandoOrdineHaStoricoStati_AlloraMappaStoricoOrdinatoPerDataDecrescente()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        var orderId = SeedOrder(dbContext);
        dbContext.OrderStatusHistory.AddRange(
            new OrderStatusHistoryEntity
            {
                OrderId = orderId,
                FromStatus = null,
                ToStatus = (int)OrderStatus.Pending,
                ChangedAt = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc),
                ChangedBy = "cliente@example.com",
                Reason = "Order created"
            },
            new OrderStatusHistoryEntity
            {
                OrderId = orderId,
                FromStatus = (int)OrderStatus.Pending,
                ToStatus = (int)OrderStatus.PaymentPending,
                ChangedAt = new DateTime(2026, 4, 2, 10, 30, 0, DateTimeKind.Utc),
                ChangedBy = "admin@micene.it",
                Reason = "Avvio pagamento"
            });
        dbContext.SaveChanges();
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetOrders(pageSize: 0);

        // Assert
        var storico = risultato.Items.Single().StatusHistory;
        storico.Should().HaveCount(2);
        storico[0].Should().BeEquivalentTo(new
        {
            FromStatus = (OrderStatus?)OrderStatus.Pending,
            ToStatus = OrderStatus.PaymentPending,
            ChangedAt = new DateTime(2026, 4, 2, 10, 30, 0, DateTimeKind.Utc),
            ChangedBy = "admin@micene.it",
            Reason = "Avvio pagamento"
        });
        storico[1].Should().BeEquivalentTo(new
        {
            FromStatus = (OrderStatus?)null,
            ToStatus = OrderStatus.Pending,
            ChangedAt = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc),
            ChangedBy = "cliente@example.com",
            Reason = "Order created"
        });
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
    public void GetProduct_QuandoProdottoHaImmaginiCarousel_AlloraLeMappaOrdinate()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedProductCarouselImages(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetProduct("PRD-001");

        // Assert
        risultato.Should().NotBeNull();
        risultato!.CarouselImages.Should().SatisfyRespectively(
            prima =>
            {
                prima.ImageUrl.Should().Be("https://example.com/laptop-front-800.jpg");
                prima.AltText.Should().Be("Laptop tracer vista frontale");
                prima.DisplayOrder.Should().Be(1);
            },
            seconda =>
            {
                seconda.ImageUrl.Should().Be("https://example.com/laptop-side-800.jpg");
                seconda.AltText.Should().Be("Laptop tracer vista laterale");
                seconda.DisplayOrder.Should().Be(2);
            });
    }

    [Fact]
    public void GetAvailableProducts_QuandoProdottoHaImmaginiCarousel_AlloraIncludeCarouselOrdinato()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 10, price: 25m);
        SeedProductCarouselImages(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.GetAvailableProducts();

        // Assert
        risultato.Single().CarouselImages.Select(image => image.DisplayOrder).Should().Equal(1, 2);
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

    private static void SeedProductCarouselImages(DashboardOrdersDbContext dbContext)
    {
        var product = dbContext.Products.Single(product => product.Code == "PRD-001");
        dbContext.ProductCarouselImages.AddRange(
            new ProductCarouselImageEntity
            {
                ProductId = product.Id,
                ImageUrl = "https://example.com/laptop-side-800.jpg",
                AltText = "Laptop tracer vista laterale",
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new ProductCarouselImageEntity
            {
                ProductId = product.Id,
                ImageUrl = "https://example.com/laptop-front-800.jpg",
                AltText = "Laptop tracer vista frontale",
                DisplayOrder = 1,
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

    private static int SeedOrder(
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
        return order.Id;
    }

    private static void SeedInitialStatusHistory(DashboardOrdersDbContext dbContext, int orderId, OrderStatus status)
    {
        dbContext.OrderStatusHistory.Add(new OrderStatusHistoryEntity
        {
            OrderId = orderId,
            FromStatus = null,
            ToStatus = (int)status,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "seed@test.local",
            Reason = "Seed initial state"
        });
        dbContext.SaveChanges();
    }
}
