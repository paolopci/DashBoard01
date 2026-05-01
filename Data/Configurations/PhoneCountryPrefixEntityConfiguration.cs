using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class PhoneCountryPrefixEntityConfiguration : IEntityTypeConfiguration<PhoneCountryPrefixEntity>
{
    public void Configure(EntityTypeBuilder<PhoneCountryPrefixEntity> builder)
    {
        builder.ToTable("PhoneCountryPrefixes");
        builder.HasKey(prefix => prefix.Id);
        builder.HasIndex(prefix => prefix.Iso2).IsUnique();
        builder.HasIndex(prefix => prefix.DialCode);
        builder.Property(prefix => prefix.Iso2).HasMaxLength(2).IsRequired();
        builder.Property(prefix => prefix.Iso3).HasMaxLength(3).IsRequired();
        builder.Property(prefix => prefix.CountryName).HasMaxLength(120).IsRequired();
        builder.Property(prefix => prefix.LocalizedCountryName).HasMaxLength(120).IsRequired();
        builder.Property(prefix => prefix.DialCode).HasMaxLength(8).IsRequired();
        builder.Property(prefix => prefix.FlagPath).HasMaxLength(200).IsRequired();
        builder.Property(prefix => prefix.DisplayOrder).IsRequired();
        builder.Property(prefix => prefix.IsActive).IsRequired();
    }
}
