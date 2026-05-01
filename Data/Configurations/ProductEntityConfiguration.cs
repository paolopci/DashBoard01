using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.HasIndex(product => product.Code).IsUnique();
        builder.HasIndex(product => product.CategoryCode);
        builder.Property(product => product.Code).HasMaxLength(30).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(100).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(500);
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.CategoryCode).HasMaxLength(20).IsRequired();
        builder.Property(product => product.ImageUrl).HasMaxLength(200);

        builder
            .HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
