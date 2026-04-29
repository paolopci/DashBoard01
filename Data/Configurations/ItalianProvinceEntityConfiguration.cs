using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ItalianProvinceEntityConfiguration : IEntityTypeConfiguration<ItalianProvinceEntity>
{
    public void Configure(EntityTypeBuilder<ItalianProvinceEntity> builder)
    {
        builder.ToTable("ItalianProvinces");
        builder.HasKey(province => province.Code);
        builder.HasIndex(province => province.Name);
        builder.HasIndex(province => province.Abbreviation);
        builder.Property(province => province.Code).HasMaxLength(3).IsRequired();
        builder.Property(province => province.RegionCode).HasMaxLength(2).IsRequired();
        builder.Property(province => province.Name).HasMaxLength(100).IsRequired();
        builder.Property(province => province.Abbreviation).HasMaxLength(4);
        builder.Property(province => province.Nuts3Code).HasMaxLength(5);

        builder
            .HasOne(province => province.Region)
            .WithMany(region => region.Provinces)
            .HasForeignKey(province => province.RegionCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
