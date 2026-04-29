using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class CartEntityConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(cart => cart.Id);
        builder.HasIndex(cart => cart.CustomerEmail).IsUnique();
        builder.HasIndex(cart => cart.ExpiresAt);
        builder.Property(cart => cart.CustomerEmail).HasMaxLength(256).IsRequired();
        builder.Property(cart => cart.CreatedAt).IsRequired();
        builder.Property(cart => cart.UpdatedAt).IsRequired();
        builder.Property(cart => cart.ExpiresAt).IsRequired();
    }
}
