using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DashboardOrders.Tests;

public class CheckoutDataServiceTests
{
    [Fact]
    public void StartCheckout_QuandoCarrelloVuoto_AlloraNonCreaSessione()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var risultato = sut.StartCheckout("cliente@test.it");

        // Assert
        risultato.Should().BeFalse();
        dbContext.CheckoutSessions.Should().BeEmpty();
    }

    [Fact]
    public void SaveCheckoutAddresses_QuandoDatiValidi_AlloraAggiornaSessione()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();

        // Act
        var risultato = sut.SaveCheckoutAddresses("cliente@test.it", CreateAddresses());
        var checkout = sut.GetCheckout("cliente@test.it");

        // Assert
        risultato.Should().BeTrue();
        checkout.Should().NotBeNull();
        checkout!.ShippingFullName.Should().Be("Mario Rossi");
        checkout.ShippingCity.Should().Be("Milano");
        checkout.BillingSameAsShipping.Should().BeTrue();
        checkout.CurrentStep.Should().Be(CheckoutStep.Addresses);
    }

    [Fact]
    public void ConfirmCheckout_QuandoStockInsufficiente_AlloraNonCreaOrdineEPreservaCarrello()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 2, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        sut.SaveCheckoutAddresses("cliente@test.it", CreateAddresses()).Should().BeTrue();
        sut.SaveCheckoutOptions("cliente@test.it", new CheckoutOptionsViewModel
        {
            DeliveryMethod = "standard",
            PaymentMethod = "test-card"
        }).Should().BeTrue();
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity = 1;
        dbContext.SaveChanges();

        // Act
        var risultato = sut.ConfirmCheckout("cliente@test.it");

        // Assert
        risultato.Success.Should().BeFalse();
        risultato.ErrorMessage.Should().Contain("disponibil");
        dbContext.Orders.Should().BeEmpty();
        sut.GetCart("cliente@test.it").TotalItems.Should().Be(2);
    }

    [Fact]
    public void ConfirmCheckout_ConPagamentoTest_AlloraCreaOrdinePaymentPendingESvuotaCarrello()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 2).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        sut.SaveCheckoutAddresses("cliente@test.it", CreateAddresses()).Should().BeTrue();
        sut.SaveCheckoutOptions("cliente@test.it", new CheckoutOptionsViewModel
        {
            DeliveryMethod = "standard",
            PaymentMethod = "test-card"
        }).Should().BeTrue();

        // Act
        var risultato = sut.ConfirmCheckout("cliente@test.it");

        // Assert
        risultato.Success.Should().BeTrue();
        risultato.RequiresPayment.Should().BeTrue();
        risultato.OrderId.Should().BeGreaterThan(0);
        dbContext.Orders.Single().Status.Should().Be((int)OrderStatus.PaymentPending);
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(3);
        sut.GetCart("cliente@test.it").Items.Should().BeEmpty();
        dbContext.OrderCheckoutDetails.Should().ContainSingle(details =>
            details.OrderId == risultato.OrderId &&
            details.PaymentMethod == "test-card" &&
            details.PaymentStatus == "pending");
    }

    [Fact]
    public void ProcessTestPayment_QuandoPagamentoRiuscito_AlloraAutorizzaEConfermaOrdine()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        sut.SaveCheckoutAddresses("cliente@test.it", CreateAddresses()).Should().BeTrue();
        sut.SaveCheckoutOptions("cliente@test.it", new CheckoutOptionsViewModel
        {
            DeliveryMethod = "standard",
            PaymentMethod = "test-card"
        }).Should().BeTrue();
        var orderId = sut.ConfirmCheckout("cliente@test.it").OrderId!.Value;

        // Act
        var risultato = sut.ProcessTestPayment("cliente@test.it", orderId, TestPaymentOutcome.Authorized);

        // Assert
        risultato.Success.Should().BeTrue();
        risultato.FinalStatus.Should().Be(OrderStatus.Confirmed);
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.Confirmed);
        dbContext.OrderCheckoutDetails.Single(details => details.OrderId == orderId).PaymentStatus.Should().Be("authorized");
        dbContext.OrderCheckoutDetails.Single(details => details.OrderId == orderId).TestTransactionReference.Should().NotBeNullOrWhiteSpace();
        dbContext.OrderStatusHistory.Should().Contain(history =>
            history.ToStatus == (int)OrderStatus.PaymentAuthorized &&
            history.ChangedBy == "cliente@test.it");
        dbContext.OrderStatusHistory.Should().Contain(history =>
            history.ToStatus == (int)OrderStatus.Confirmed &&
            history.ChangedBy == "cliente@test.it");
    }

    [Fact]
    public void ConfirmCheckout_ConMetodoPending_AlloraCreaOrdinePendingSenzaPagamento()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        sut.SaveCheckoutAddresses("cliente@test.it", CreateAddresses()).Should().BeTrue();
        sut.SaveCheckoutOptions("cliente@test.it", new CheckoutOptionsViewModel
        {
            DeliveryMethod = "pickup",
            PaymentMethod = "pending"
        }).Should().BeTrue();

        // Act
        var risultato = sut.ConfirmCheckout("cliente@test.it");

        // Assert
        risultato.Success.Should().BeTrue();
        risultato.RequiresPayment.Should().BeFalse();
        dbContext.Orders.Single().Status.Should().Be((int)OrderStatus.Pending);
        dbContext.OrderCheckoutDetails.Single().PaymentStatus.Should().Be("not-required");
    }

    private static CheckoutAddressesViewModel CreateAddresses()
    {
        return new CheckoutAddressesViewModel
        {
            ShippingFullName = "Mario Rossi",
            ShippingAddressLine = "Via Roma 1",
            ShippingCity = "Milano",
            ShippingPostalCode = "20100",
            ShippingCountry = "Italia",
            ShippingPhone = "021234567",
            BillingSameAsShipping = true,
            BillingVatNumber = "TEST-VAT"
        };
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
        if (!dbContext.Categories.Any(category => category.Code == "CAT-001"))
        {
            dbContext.Categories.Add(new CategoryEntity
            {
                Code = "CAT-001",
                Name = "Informatica",
                Description = "Prodotti informatici"
            });
        }

        dbContext.Products.Add(new ProductEntity
        {
            Code = "PRD-001",
            Name = "Laptop checkout",
            Description = "Prodotto per checkout",
            Price = price,
            StockQuantity = stockQuantity,
            CategoryCode = "CAT-001",
            ImageUrl = "https://loremflickr.com/320/240/laptop,computer/all?lock=checkout",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }
}
