using System.Reflection;
using DashboardOrders.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Data;

public class DashboardOrdersDbContext(DbContextOptions<DashboardOrdersDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OrderStatusHistoryEntity> OrderStatusHistory => Set<OrderStatusHistoryEntity>();
    public DbSet<CartEntity> Carts => Set<CartEntity>();
    public DbSet<CartItemEntity> CartItems => Set<CartItemEntity>();
    public DbSet<CheckoutSessionEntity> CheckoutSessions => Set<CheckoutSessionEntity>();
    public DbSet<OrderCheckoutDetailsEntity> OrderCheckoutDetails => Set<OrderCheckoutDetailsEntity>();
    public DbSet<ItalianPostalCodeEntity> ItalianPostalCodes => Set<ItalianPostalCodeEntity>();
    public DbSet<ItalianRegionEntity> ItalianRegions => Set<ItalianRegionEntity>();
    public DbSet<ItalianProvinceEntity> ItalianProvinces => Set<ItalianProvinceEntity>();
    public DbSet<ItalianMunicipalityEntity> ItalianMunicipalities => Set<ItalianMunicipalityEntity>();
    public DbSet<ProductCarouselImageEntity> ProductCarouselImages => Set<ProductCarouselImageEntity>();
    public DbSet<PhoneCountryPrefixEntity> PhoneCountryPrefixes => Set<PhoneCountryPrefixEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
