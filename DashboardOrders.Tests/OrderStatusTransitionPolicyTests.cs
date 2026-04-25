using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Xunit;

namespace DashboardOrders.Tests;

public class OrderStatusTransitionPolicyTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.PaymentPending)]
    [InlineData(OrderStatus.PaymentPending, OrderStatus.PaymentAuthorized)]
    [InlineData(OrderStatus.PaymentAuthorized, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Processing)]
    [InlineData(OrderStatus.Processing, OrderStatus.Picking)]
    [InlineData(OrderStatus.Picking, OrderStatus.Packing)]
    [InlineData(OrderStatus.Packing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public void CanTransition_QuandoTransizioneConsentita_AlloraRestituisceTrue(OrderStatus fromStatus, OrderStatus toStatus)
    {
        // Act
        var risultato = OrderStatusTransitionPolicy.CanTransition(fromStatus, toStatus);

        // Assert
        risultato.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing)]
    [InlineData(OrderStatus.PaymentFailed, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]
    public void CanTransition_QuandoTransizioneVietata_AlloraRestituisceFalse(OrderStatus fromStatus, OrderStatus toStatus)
    {
        // Act
        var risultato = OrderStatusTransitionPolicy.CanTransition(fromStatus, toStatus);

        // Assert
        risultato.Should().BeFalse();
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Refunded, true)]
    [InlineData(OrderStatus.PartiallyRefunded, true)]
    [InlineData(OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Delivered, false)]
    public void IsTerminal_QuandoInvocato_AlloraRestituisceValoreAtteso(OrderStatus status, bool atteso)
    {
        // Act
        var risultato = OrderStatusTransitionPolicy.IsTerminal(status);

        // Assert
        risultato.Should().Be(atteso);
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.PaymentFailed, true)]
    [InlineData(OrderStatus.Refunded, true)]
    [InlineData(OrderStatus.PartiallyRefunded, true)]
    [InlineData(OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Shipped, false)]
    public void RequiresReason_QuandoInvocato_AlloraRestituisceValoreAtteso(OrderStatus status, bool atteso)
    {
        // Act
        var risultato = OrderStatusTransitionPolicy.RequiresReason(status);

        // Assert
        risultato.Should().Be(atteso);
    }
}
