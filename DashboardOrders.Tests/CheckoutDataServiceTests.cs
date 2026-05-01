using DashboardOrders.Models.ViewModels;
using DashboardOrders.Data;
using DashboardOrders.Domain.Entities;
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
    public void GetItalianProvinces_QuandoLookupPresenti_AlloraRestituisceProvinceDistinteOrdinate()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedItalianAdministrativeTerritories(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var province = sut.GetItalianProvinces();

        // Assert
        province.Should().Equal("Ancona", "Pesaro e Urbino");
    }

    [Fact]
    public void GetItalianCities_QuandoProvinciaIndicata_AlloraRestituisceCittaDistinteOrdinate()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedItalianAdministrativeTerritories(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var citta = sut.GetItalianCities("Pesaro e Urbino");

        // Assert
        citta.Should().Equal("Fano", "Pesaro");
    }

    [Fact]
    public void GetItalianPostalCodes_QuandoCittaMultiCap_AlloraRestituisceCapDisponibili()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedItalianAdministrativeTerritories(dbContext);
        SeedItalianPostalCodes(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var cap = sut.GetItalianPostalCodes("Pesaro e Urbino", "Pesaro");

        // Assert
        cap.Should().Equal("61121", "61122");
    }

    [Fact]
    public void GetPhoneCountryPrefixes_QuandoLookupPresenti_AlloraRestituisceSoloAttiviOrdinati()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedPhoneCountryPrefixes(dbContext);
        var sut = new DashboardOrdersDataService(dbContext);

        // Act
        var prefissi = sut.GetPhoneCountryPrefixes();

        // Assert
        prefissi.Select(prefix => prefix.Iso2).Should().Equal("IT", "GB", "US");
        prefissi.Select(prefix => prefix.DialCode).Should().Equal("+39", "+44", "+1");
        prefissi.Should().NotContain(prefix => prefix.Iso2 == "FR");
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
    public void SaveCheckoutAddresses_QuandoCampiStrutturatiEFatturazioneUguale_AlloraComponeECopiaIndirizzi()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        var model = new CheckoutAddressesViewModel
        {
            ShippingLastName = "Rossi",
            ShippingFirstName = "Mario",
            ShippingPhonePrefix = "+39",
            ShippingPhoneNumber = "3281234567",
            ShippingStreet = "Via Roma",
            ShippingStreetNumber = "1",
            ShippingProvince = "Pesaro e Urbino",
            ShippingCity = "Pesaro",
            ShippingPostalCode = "61121",
            BillingSameAsShipping = true
        };

        // Act
        var risultato = sut.SaveCheckoutAddresses("cliente@test.it", model);
        var checkout = sut.GetCheckout("cliente@test.it");

        // Assert
        risultato.Should().BeTrue();
        checkout.Should().NotBeNull();
        checkout!.ShippingFullName.Should().Be("Rossi Mario");
        checkout.ShippingPhone.Should().Be("+39 3281234567");
        checkout.ShippingAddressLine.Should().Be("Via Roma 1");
        checkout.ShippingCountry.Should().Be("Italia");
        checkout.BillingFullName.Should().Be(checkout.ShippingFullName);
        checkout.BillingAddressLine.Should().Be(checkout.ShippingAddressLine);
        checkout.BillingCity.Should().Be(checkout.ShippingCity);
        checkout.BillingPostalCode.Should().Be(checkout.ShippingPostalCode);
        checkout.BillingCountry.Should().Be("Italia");
    }

    [Fact]
    public void SaveCheckoutAddresses_QuandoPrefissoEsteroSelezionato_AlloraSalvaPaeseAssociato()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedPhoneCountryPrefixes(dbContext);
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        var model = new CheckoutAddressesViewModel
        {
            ShippingLastName = "Smith",
            ShippingFirstName = "John",
            ShippingPhonePrefix = "+44",
            ShippingPhoneCountryIso2 = "GB",
            ShippingPhoneNumber = "7123456789",
            ShippingStreet = "Via Roma",
            ShippingStreetNumber = "1",
            ShippingProvince = "Pesaro e Urbino",
            ShippingCity = "Pesaro",
            ShippingPostalCode = "61121",
            BillingSameAsShipping = true
        };

        // Act
        var risultato = sut.SaveCheckoutAddresses("cliente@test.it", model);
        var checkout = sut.GetCheckout("cliente@test.it");

        // Assert
        risultato.Should().BeTrue();
        checkout.Should().NotBeNull();
        checkout!.ShippingPhone.Should().Be("+44 7123456789");
        checkout.ShippingCountry.Should().Be("Regno Unito");
        checkout.BillingCountry.Should().Be("Regno Unito");
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

    [Fact]
    public void ConfirmCheckout_ConStripeTest_AlloraCreaOrdinePaymentPendingESvuotaCarrello()
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
            PaymentMethod = "stripe-test"
        }).Should().BeTrue();

        // Act
        var risultato = sut.ConfirmCheckout("cliente@test.it");

        // Assert
        risultato.Success.Should().BeTrue();
        risultato.RequiresPayment.Should().BeTrue();
        risultato.RequiresStripeCheckout.Should().BeTrue();
        risultato.PaymentMethod.Should().Be("stripe-test");
        dbContext.Orders.Single().Status.Should().Be((int)OrderStatus.PaymentPending);
        dbContext.OrderCheckoutDetails.Should().ContainSingle(details =>
            details.OrderId == risultato.OrderId &&
            details.PaymentMethod == "stripe-test" &&
            details.PaymentStatus == "pending");
        sut.GetCart("cliente@test.it").Items.Should().BeEmpty();
    }

    [Fact]
    public void SaveStripeCheckoutSession_QuandoOrdineStripeValido_AlloraSalvaRiferimenti()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = CreateStripeCheckoutOrder(dbContext, out var orderId);

        // Act
        var risultato = sut.SaveStripeCheckoutSession("cliente@test.it", orderId, new StripeCheckoutSessionResult
        {
            SessionId = "cs_test_123",
            Url = "https://checkout.stripe.com/c/pay/cs_test_123",
            PaymentIntentId = "pi_123",
            PaymentStatus = "unpaid"
        });

        // Assert
        risultato.Should().BeTrue();
        sut.GetOrderIdByStripeCheckoutSession("cs_test_123").Should().Be(orderId);
        var details = dbContext.OrderCheckoutDetails.Single(details => details.OrderId == orderId);
        details.StripeCheckoutSessionId.Should().Be("cs_test_123");
        details.StripePaymentIntentId.Should().Be("pi_123");
        details.StripePaymentStatus.Should().Be("unpaid");
    }

    [Fact]
    public void CompleteStripePayment_QuandoSessionePagata_AlloraAutorizzaEConfermaInModoIdempotente()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = CreateStripeCheckoutOrder(dbContext, out var orderId);
        sut.SaveStripeCheckoutSession("cliente@test.it", orderId, new StripeCheckoutSessionResult
        {
            SessionId = "cs_test_paid",
            Url = "https://checkout.stripe.com/c/pay/cs_test_paid",
            PaymentIntentId = "pi_initial",
            PaymentStatus = "unpaid"
        }).Should().BeTrue();

        // Act
        var primoRisultato = sut.CompleteStripePayment("cs_test_paid", "pi_paid", "paid", "stripe");
        var secondoRisultato = sut.CompleteStripePayment("cs_test_paid", "pi_paid", "paid", "stripe");

        // Assert
        primoRisultato.Success.Should().BeTrue();
        secondoRisultato.Success.Should().BeTrue();
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.Confirmed);
        dbContext.OrderCheckoutDetails.Single(details => details.OrderId == orderId).PaymentStatus.Should().Be("authorized");
        dbContext.OrderStatusHistory.Count(history => history.OrderId == orderId && history.ToStatus == (int)OrderStatus.PaymentAuthorized).Should().Be(1);
        dbContext.OrderStatusHistory.Count(history => history.OrderId == orderId && history.ToStatus == (int)OrderStatus.Confirmed).Should().Be(1);
    }

    [Fact]
    public void CompleteStripePayment_QuandoRequesterNonProprietario_AlloraNonCompletaPagamento()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = CreateStripeCheckoutOrder(dbContext, out var orderId);
        sut.SaveStripeCheckoutSession("cliente@test.it", orderId, new StripeCheckoutSessionResult
        {
            SessionId = "cs_test_other_user",
            Url = "https://checkout.stripe.com/c/pay/cs_test_other_user",
            PaymentIntentId = "pi_initial",
            PaymentStatus = "unpaid"
        }).Should().BeTrue();

        // Act
        var risultato = sut.CompleteStripePayment("cs_test_other_user", "pi_paid", "paid", "altro@test.it", "altro@test.it");

        // Assert
        risultato.Success.Should().BeFalse();
        risultato.ErrorMessage.Should().Be("Ordine Stripe non trovato.");
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.PaymentPending);
        dbContext.OrderCheckoutDetails.Single(details => details.OrderId == orderId).PaymentStatus.Should().Be("pending");
    }

    [Fact]
    public void FailStripePayment_QuandoSessioneFallita_AlloraFallisceERipristinaStockUnaSolaVolta()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = CreateStripeCheckoutOrder(dbContext, out var orderId);
        sut.SaveStripeCheckoutSession("cliente@test.it", orderId, new StripeCheckoutSessionResult
        {
            SessionId = "cs_test_failed",
            Url = "https://checkout.stripe.com/c/pay/cs_test_failed",
            PaymentIntentId = "pi_initial",
            PaymentStatus = "unpaid"
        }).Should().BeTrue();

        // Act
        var primoRisultato = sut.FailStripePayment("cs_test_failed", "pi_failed", "failed", "stripe");
        var secondoRisultato = sut.FailStripePayment("cs_test_failed", "pi_failed", "failed", "stripe");

        // Assert
        primoRisultato.Success.Should().BeTrue();
        secondoRisultato.Success.Should().BeTrue();
        dbContext.Orders.Single(order => order.Id == orderId).Status.Should().Be((int)OrderStatus.PaymentFailed);
        dbContext.Products.Single(product => product.Code == "PRD-001").StockQuantity.Should().Be(5);
        dbContext.OrderStatusHistory.Count(history => history.OrderId == orderId && history.ToStatus == (int)OrderStatus.PaymentFailed).Should().Be(1);
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

    private static DashboardOrdersDataService CreateStripeCheckoutOrder(DashboardOrdersDbContext dbContext, out int orderId)
    {
        SeedProduct(dbContext, stockQuantity: 5, price: 25m);
        var sut = new DashboardOrdersDataService(dbContext);
        sut.AddOrUpdateCartItem("cliente@test.it", "PRD-001", 1).Should().BeTrue();
        sut.StartCheckout("cliente@test.it").Should().BeTrue();
        sut.SaveCheckoutAddresses("cliente@test.it", CreateAddresses()).Should().BeTrue();
        sut.SaveCheckoutOptions("cliente@test.it", new CheckoutOptionsViewModel
        {
            DeliveryMethod = "standard",
            PaymentMethod = "stripe-test"
        }).Should().BeTrue();
        orderId = sut.ConfirmCheckout("cliente@test.it").OrderId!.Value;
        return sut;
    }

    private static void SeedItalianPostalCodes(DashboardOrdersDbContext dbContext)
    {
        dbContext.ItalianPostalCodes.AddRange(
            new ItalianPostalCodeEntity
            {
                ProvinceName = "Pesaro e Urbino",
                ProvinceCode = "PU",
                CityName = "Pesaro",
                PostalCode = "61122"
            },
            new ItalianPostalCodeEntity
            {
                ProvinceName = "Ancona",
                ProvinceCode = "AN",
                CityName = "Ancona",
                PostalCode = "60121"
            },
            new ItalianPostalCodeEntity
            {
                ProvinceName = "Pesaro e Urbino",
                ProvinceCode = "PU",
                CityName = "Fano",
                PostalCode = "61032"
            },
            new ItalianPostalCodeEntity
            {
                ProvinceName = "Pesaro e Urbino",
                ProvinceCode = "PU",
                CityName = "Pesaro",
                PostalCode = "61121"
            });
        dbContext.SaveChanges();
    }

    private static void SeedPhoneCountryPrefixes(DashboardOrdersDbContext dbContext)
    {
        dbContext.PhoneCountryPrefixes.AddRange(
            new PhoneCountryPrefixEntity
            {
                Iso2 = "US",
                Iso3 = "USA",
                CountryName = "United States",
                LocalizedCountryName = "Stati Uniti",
                DialCode = "+1",
                FlagPath = "/img/flags/4x3/us.svg",
                DisplayOrder = 1000,
                IsActive = true
            },
            new PhoneCountryPrefixEntity
            {
                Iso2 = "IT",
                Iso3 = "ITA",
                CountryName = "Italy",
                LocalizedCountryName = "Italia",
                DialCode = "+39",
                FlagPath = "/img/flags/4x3/it.svg",
                DisplayOrder = 0,
                IsActive = true
            },
            new PhoneCountryPrefixEntity
            {
                Iso2 = "GB",
                Iso3 = "GBR",
                CountryName = "United Kingdom",
                LocalizedCountryName = "Regno Unito",
                DialCode = "+44",
                FlagPath = "/img/flags/4x3/gb.svg",
                DisplayOrder = 1000,
                IsActive = true
            },
            new PhoneCountryPrefixEntity
            {
                Iso2 = "FR",
                Iso3 = "FRA",
                CountryName = "France",
                LocalizedCountryName = "Francia",
                DialCode = "+33",
                FlagPath = "/img/flags/4x3/fr.svg",
                DisplayOrder = 1000,
                IsActive = false
            });
        dbContext.SaveChanges();
    }

    private static void SeedItalianAdministrativeTerritories(DashboardOrdersDbContext dbContext)
    {
        dbContext.ItalianRegions.Add(new ItalianRegionEntity
        {
            Code = "11",
            Name = "Marche",
            Nuts1Code = "ITI",
            Nuts2Code = "ITI3"
        });
        dbContext.ItalianProvinces.AddRange(
            new ItalianProvinceEntity
            {
                Code = "042",
                RegionCode = "11",
                Name = "Ancona",
                Abbreviation = "AN",
                Nuts3Code = "ITI32"
            },
            new ItalianProvinceEntity
            {
                Code = "041",
                RegionCode = "11",
                Name = "Pesaro e Urbino",
                Abbreviation = "PU",
                Nuts3Code = "ITI31"
            });
        dbContext.ItalianMunicipalities.AddRange(
            new ItalianMunicipalityEntity
            {
                Code = "042002",
                ProvinceCode = "042",
                RegionCode = "11",
                Name = "Ancona",
                CadastralCode = "A271",
                IsProvinceCapital = true
            },
            new ItalianMunicipalityEntity
            {
                Code = "041013",
                ProvinceCode = "041",
                RegionCode = "11",
                Name = "Fano",
                CadastralCode = "D488"
            },
            new ItalianMunicipalityEntity
            {
                Code = "041044",
                ProvinceCode = "041",
                RegionCode = "11",
                Name = "Pesaro",
                CadastralCode = "G479",
                IsProvinceCapital = true
            });
        dbContext.SaveChanges();
    }
}
