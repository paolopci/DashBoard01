using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class CheckoutSessionEntityConfiguration : IEntityTypeConfiguration<CheckoutSessionEntity>
{
    public void Configure(EntityTypeBuilder<CheckoutSessionEntity> builder)
    {
        builder.ToTable("CheckoutSessions");
        builder.HasKey(session => session.Id);
        builder.HasIndex(session => session.CustomerEmail).IsUnique();
        builder.HasIndex(session => session.ExpiresAt);
        builder.Property(session => session.CustomerEmail).HasMaxLength(256).IsRequired();
        builder.Property(session => session.TotalAmount).HasPrecision(18, 2);
        builder.Property(session => session.ShippingFullName).HasMaxLength(120);
        builder.Property(session => session.ShippingAddressLine).HasMaxLength(200);
        builder.Property(session => session.ShippingCity).HasMaxLength(100);
        builder.Property(session => session.ShippingPostalCode).HasMaxLength(20);
        builder.Property(session => session.ShippingCountry).HasMaxLength(100);
        builder.Property(session => session.ShippingPhone).HasMaxLength(30);
        builder.Property(session => session.BillingFullName).HasMaxLength(120);
        builder.Property(session => session.BillingAddressLine).HasMaxLength(200);
        builder.Property(session => session.BillingCity).HasMaxLength(100);
        builder.Property(session => session.BillingPostalCode).HasMaxLength(20);
        builder.Property(session => session.BillingCountry).HasMaxLength(100);
        builder.Property(session => session.BillingVatNumber).HasMaxLength(40);
        builder.Property(session => session.DeliveryMethod).HasMaxLength(30).IsRequired();
        builder.Property(session => session.PaymentMethod).HasMaxLength(30).IsRequired();
    }
}
