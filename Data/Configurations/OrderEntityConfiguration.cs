using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.HasIndex(order => order.CustomerId);
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.Property(order => order.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2);
        builder.Property(order => order.Notes).HasMaxLength(500);

        builder
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
