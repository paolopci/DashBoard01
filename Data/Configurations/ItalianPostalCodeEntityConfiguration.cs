using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ItalianPostalCodeEntityConfiguration : IEntityTypeConfiguration<ItalianPostalCodeEntity>
{
    public void Configure(EntityTypeBuilder<ItalianPostalCodeEntity> builder)
    {
        builder.ToTable("ItalianPostalCodes");
        builder.HasKey(postalCode => postalCode.Id);
        builder.HasIndex(postalCode => postalCode.ProvinceName);
        builder.HasIndex(postalCode => postalCode.CityName);
        builder.HasIndex(postalCode => new { postalCode.ProvinceName, postalCode.CityName, postalCode.PostalCode }).IsUnique();
        builder.Property(postalCode => postalCode.ProvinceName).HasMaxLength(100).IsRequired();
        builder.Property(postalCode => postalCode.ProvinceCode).HasMaxLength(4).IsRequired();
        builder.Property(postalCode => postalCode.CityName).HasMaxLength(100).IsRequired();
        builder.Property(postalCode => postalCode.PostalCode).HasMaxLength(10).IsRequired();
    }
}
