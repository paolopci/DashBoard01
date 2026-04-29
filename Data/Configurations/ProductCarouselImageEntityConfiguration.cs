using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ProductCarouselImageEntityConfiguration : IEntityTypeConfiguration<ProductCarouselImageEntity>
{
    public void Configure(EntityTypeBuilder<ProductCarouselImageEntity> builder)
    {
        builder.ToTable("ProductCarouselImages");
        builder.HasKey(image => image.Id);
        builder.HasIndex(image => image.ImageUrl).IsUnique();
        builder.HasIndex(image => new { image.ProductId, image.DisplayOrder }).IsUnique();
        builder.Property(image => image.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(image => image.AltText).HasMaxLength(200).IsRequired();
        builder.Property(image => image.DisplayOrder).IsRequired();
        builder.Property(image => image.CreatedAt).IsRequired();

        builder
            .HasOne(image => image.Product)
            .WithMany(product => product.CarouselImages)
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
