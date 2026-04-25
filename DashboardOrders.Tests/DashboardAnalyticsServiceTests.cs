using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DashboardOrders.Tests;

public class DashboardAnalyticsServiceTests
{
    [Fact]
    public void GetAnalytics_QuandoPresentiStatiNonRevenueRelevant_AlloraEscludeDaRicavoCancelledPaymentFailedERefunded()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        SeedCustomer(dbContext);
        SeedOrder(dbContext, "ORD-001", 100m, OrderStatus.Pending);
        SeedOrder(dbContext, "ORD-002", 200m, OrderStatus.Cancelled);
        SeedOrder(dbContext, "ORD-003", 300m, OrderStatus.PaymentFailed);
        SeedOrder(dbContext, "ORD-004", 400m, OrderStatus.Refunded);
        SeedOrder(dbContext, "ORD-005", 500m, OrderStatus.Delivered);
        var sut = new DashboardAnalyticsService(dbContext);

        // Act
        var risultato = sut.GetAnalytics();

        // Assert
        risultato.Metrics.Should().Contain(metric =>
            metric.Name == "Ricavo" &&
            metric.Value == 600m.ToString("C"));
    }

    private static DashboardOrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DashboardOrdersDbContext(options);
    }

    private static void SeedCustomer(DashboardOrdersDbContext dbContext)
    {
        dbContext.Customers.Add(new CustomerEntity
        {
            Name = "Mario Rossi",
            Email = "mario.rossi@example.com",
            Phone = "+390212345678",
            AvatarInitials = "MR",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }

    private static void SeedOrder(DashboardOrdersDbContext dbContext, string orderNumber, decimal totalAmount, OrderStatus status)
    {
        var customerId = dbContext.Customers.Single().Id;
        dbContext.Orders.Add(new OrderEntity
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Status = (int)status,
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }
}
