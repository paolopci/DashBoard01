using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class OrderCheckoutDetailsEntityConfiguration : IEntityTypeConfiguration<OrderCheckoutDetailsEntity>
{
    public void Configure(EntityTypeBuilder<OrderCheckoutDetailsEntity> builder)
    {
        builder.ToTable("OrderCheckoutDetails");
        builder.HasKey(details => details.Id);
        builder.HasIndex(details => details.OrderId).IsUnique();
        builder.Property(details => details.ShippingFullName).HasMaxLength(120).IsRequired();
        builder.Property(details => details.ShippingAddressLine).HasMaxLength(200).IsRequired();
        builder.Property(details => details.ShippingCity).HasMaxLength(100).IsRequired();
        builder.Property(details => details.ShippingPostalCode).HasMaxLength(20).IsRequired();
        builder.Property(details => details.ShippingCountry).HasMaxLength(100).IsRequired();
        builder.Property(details => details.ShippingPhone).HasMaxLength(30).IsRequired();
        builder.Property(details => details.BillingFullName).HasMaxLength(120).IsRequired();
        builder.Property(details => details.BillingAddressLine).HasMaxLength(200).IsRequired();
        builder.Property(details => details.BillingCity).HasMaxLength(100).IsRequired();
        builder.Property(details => details.BillingPostalCode).HasMaxLength(20).IsRequired();
        builder.Property(details => details.BillingCountry).HasMaxLength(100).IsRequired();
        builder.Property(details => details.BillingVatNumber).HasMaxLength(40);
        builder.Property(details => details.DeliveryMethod).HasMaxLength(30).IsRequired();
        builder.Property(details => details.PaymentMethod).HasMaxLength(30).IsRequired();
        builder.Property(details => details.PaymentStatus).HasMaxLength(30).IsRequired();
        builder.Property(details => details.TestTransactionReference).HasMaxLength(80);
        builder.Property(details => details.StripeCheckoutSessionId).HasMaxLength(120);
        builder.Property(details => details.StripePaymentIntentId).HasMaxLength(120);
        builder.Property(details => details.StripePaymentStatus).HasMaxLength(40);
        builder.HasIndex(details => details.StripeCheckoutSessionId).IsUnique().HasFilter("[StripeCheckoutSessionId] IS NOT NULL");

        builder
            .HasOne(details => details.Order)
            .WithOne(order => order.CheckoutDetails)
            .HasForeignKey<OrderCheckoutDetailsEntity>(details => details.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
