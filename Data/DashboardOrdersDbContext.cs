using DashboardOrders.Data.Entities;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.City).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Country).HasMaxLength(100).IsRequired();
            entity.Property(user => user.FiscalCode).HasMaxLength(16).IsRequired();
            entity.HasIndex(user => user.FiscalCode).IsUnique();
        });

        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(category => category.Code);
            entity.Property(category => category.Code).HasMaxLength(20);
            entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(product => product.Id);
            entity.HasIndex(product => product.Code).IsUnique();
            entity.HasIndex(product => product.CategoryCode);
            entity.Property(product => product.Code).HasMaxLength(30).IsRequired();
            entity.Property(product => product.Name).HasMaxLength(100).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(500);
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.CategoryCode).HasMaxLength(20).IsRequired();
            entity.Property(product => product.ImageUrl).HasMaxLength(200);

            entity
                .HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductCarouselImageEntity>(entity =>
        {
            entity.ToTable("ProductCarouselImages");
            entity.HasKey(image => image.Id);
            entity.HasIndex(image => image.ImageUrl).IsUnique();
            entity.HasIndex(image => new { image.ProductId, image.DisplayOrder }).IsUnique();
            entity.Property(image => image.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(image => image.AltText).HasMaxLength(200).IsRequired();
            entity.Property(image => image.DisplayOrder).IsRequired();
            entity.Property(image => image.CreatedAt).IsRequired();

            entity
                .HasOne(image => image.Product)
                .WithMany(product => product.CarouselImages)
                .HasForeignKey(image => image.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.Phone).HasMaxLength(20).IsRequired();
            entity.Property(customer => customer.Address).HasMaxLength(200);
            entity.Property(customer => customer.AvatarInitials).HasMaxLength(5).IsRequired();
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.CustomerId);
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.Property(order => order.OrderNumber).HasMaxLength(30).IsRequired();
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
            entity.Property(order => order.Notes).HasMaxLength(500);

            entity
                .HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.OrderId);
            entity.HasIndex(item => item.ProductId);
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);

            entity
                .HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(item => item.Product)
                .WithMany(product => product.OrderItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderStatusHistoryEntity>(entity =>
        {
            entity.ToTable("OrderStatusHistory");
            entity.HasKey(history => history.Id);
            entity.HasIndex(history => history.OrderId);
            entity.HasIndex(history => history.ChangedAt);
            entity.Property(history => history.ChangedAt).IsRequired();
            entity.Property(history => history.ChangedBy).HasMaxLength(256);
            entity.Property(history => history.Reason).HasMaxLength(500);
            entity.Property(history => history.CorrelationId).HasMaxLength(100);

            entity
                .HasOne(history => history.Order)
                .WithMany(order => order.StatusHistory)
                .HasForeignKey(history => history.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderCheckoutDetailsEntity>(entity =>
        {
            entity.ToTable("OrderCheckoutDetails");
            entity.HasKey(details => details.Id);
            entity.HasIndex(details => details.OrderId).IsUnique();
            entity.Property(details => details.ShippingFullName).HasMaxLength(120).IsRequired();
            entity.Property(details => details.ShippingAddressLine).HasMaxLength(200).IsRequired();
            entity.Property(details => details.ShippingCity).HasMaxLength(100).IsRequired();
            entity.Property(details => details.ShippingPostalCode).HasMaxLength(20).IsRequired();
            entity.Property(details => details.ShippingCountry).HasMaxLength(100).IsRequired();
            entity.Property(details => details.ShippingPhone).HasMaxLength(30).IsRequired();
            entity.Property(details => details.BillingFullName).HasMaxLength(120).IsRequired();
            entity.Property(details => details.BillingAddressLine).HasMaxLength(200).IsRequired();
            entity.Property(details => details.BillingCity).HasMaxLength(100).IsRequired();
            entity.Property(details => details.BillingPostalCode).HasMaxLength(20).IsRequired();
            entity.Property(details => details.BillingCountry).HasMaxLength(100).IsRequired();
            entity.Property(details => details.BillingVatNumber).HasMaxLength(40);
            entity.Property(details => details.DeliveryMethod).HasMaxLength(30).IsRequired();
            entity.Property(details => details.PaymentMethod).HasMaxLength(30).IsRequired();
            entity.Property(details => details.PaymentStatus).HasMaxLength(30).IsRequired();
            entity.Property(details => details.TestTransactionReference).HasMaxLength(80);
            entity.Property(details => details.StripeCheckoutSessionId).HasMaxLength(120);
            entity.Property(details => details.StripePaymentIntentId).HasMaxLength(120);
            entity.Property(details => details.StripePaymentStatus).HasMaxLength(40);
            entity.HasIndex(details => details.StripeCheckoutSessionId).IsUnique().HasFilter("[StripeCheckoutSessionId] IS NOT NULL");

            entity
                .HasOne(details => details.Order)
                .WithOne(order => order.CheckoutDetails)
                .HasForeignKey<OrderCheckoutDetailsEntity>(details => details.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartEntity>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(cart => cart.Id);
            entity.HasIndex(cart => cart.CustomerEmail).IsUnique();
            entity.HasIndex(cart => cart.ExpiresAt);
            entity.Property(cart => cart.CustomerEmail).HasMaxLength(256).IsRequired();
            entity.Property(cart => cart.CreatedAt).IsRequired();
            entity.Property(cart => cart.UpdatedAt).IsRequired();
            entity.Property(cart => cart.ExpiresAt).IsRequired();
        });

        modelBuilder.Entity<CartItemEntity>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.CartId);
            entity.HasIndex(item => new { item.CartId, item.ProductId }).IsUnique();
            entity.Property(item => item.Quantity).IsRequired();
            entity.Property(item => item.CreatedAt).IsRequired();
            entity.Property(item => item.UpdatedAt).IsRequired();

            entity
                .HasOne(item => item.Cart)
                .WithMany(cart => cart.Items)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckoutSessionEntity>(entity =>
        {
            entity.ToTable("CheckoutSessions");
            entity.HasKey(session => session.Id);
            entity.HasIndex(session => session.CustomerEmail).IsUnique();
            entity.HasIndex(session => session.ExpiresAt);
            entity.Property(session => session.CustomerEmail).HasMaxLength(256).IsRequired();
            entity.Property(session => session.TotalAmount).HasPrecision(18, 2);
            entity.Property(session => session.ShippingFullName).HasMaxLength(120);
            entity.Property(session => session.ShippingAddressLine).HasMaxLength(200);
            entity.Property(session => session.ShippingCity).HasMaxLength(100);
            entity.Property(session => session.ShippingPostalCode).HasMaxLength(20);
            entity.Property(session => session.ShippingCountry).HasMaxLength(100);
            entity.Property(session => session.ShippingPhone).HasMaxLength(30);
            entity.Property(session => session.BillingFullName).HasMaxLength(120);
            entity.Property(session => session.BillingAddressLine).HasMaxLength(200);
            entity.Property(session => session.BillingCity).HasMaxLength(100);
            entity.Property(session => session.BillingPostalCode).HasMaxLength(20);
            entity.Property(session => session.BillingCountry).HasMaxLength(100);
            entity.Property(session => session.BillingVatNumber).HasMaxLength(40);
            entity.Property(session => session.DeliveryMethod).HasMaxLength(30).IsRequired();
            entity.Property(session => session.PaymentMethod).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<ItalianPostalCodeEntity>(entity =>
        {
            entity.ToTable("ItalianPostalCodes");
            entity.HasKey(postalCode => postalCode.Id);
            entity.HasIndex(postalCode => postalCode.ProvinceName);
            entity.HasIndex(postalCode => postalCode.CityName);
            entity.HasIndex(postalCode => new { postalCode.ProvinceName, postalCode.CityName, postalCode.PostalCode }).IsUnique();
            entity.Property(postalCode => postalCode.ProvinceName).HasMaxLength(100).IsRequired();
            entity.Property(postalCode => postalCode.ProvinceCode).HasMaxLength(4).IsRequired();
            entity.Property(postalCode => postalCode.CityName).HasMaxLength(100).IsRequired();
            entity.Property(postalCode => postalCode.PostalCode).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<ItalianRegionEntity>(entity =>
        {
            entity.ToTable("ItalianRegions");
            entity.HasKey(region => region.Code);
            entity.HasIndex(region => region.Name).IsUnique();
            entity.Property(region => region.Code).HasMaxLength(2).IsRequired();
            entity.Property(region => region.Name).HasMaxLength(100).IsRequired();
            entity.Property(region => region.Nuts1Code).HasMaxLength(5);
            entity.Property(region => region.Nuts2Code).HasMaxLength(5);
        });

        modelBuilder.Entity<ItalianProvinceEntity>(entity =>
        {
            entity.ToTable("ItalianProvinces");
            entity.HasKey(province => province.Code);
            entity.HasIndex(province => province.Name);
            entity.HasIndex(province => province.Abbreviation);
            entity.Property(province => province.Code).HasMaxLength(3).IsRequired();
            entity.Property(province => province.RegionCode).HasMaxLength(2).IsRequired();
            entity.Property(province => province.Name).HasMaxLength(100).IsRequired();
            entity.Property(province => province.Abbreviation).HasMaxLength(4);
            entity.Property(province => province.Nuts3Code).HasMaxLength(5);

            entity
                .HasOne(province => province.Region)
                .WithMany(region => region.Provinces)
                .HasForeignKey(province => province.RegionCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ItalianMunicipalityEntity>(entity =>
        {
            entity.ToTable("ItalianMunicipalities");
            entity.HasKey(municipality => municipality.Code);
            entity.HasIndex(municipality => municipality.Name);
            entity.HasIndex(municipality => municipality.ProvinceCode);
            entity.HasIndex(municipality => new { municipality.ProvinceCode, municipality.Name });
            entity.Property(municipality => municipality.Code).HasMaxLength(6).IsRequired();
            entity.Property(municipality => municipality.ProvinceCode).HasMaxLength(3).IsRequired();
            entity.Property(municipality => municipality.RegionCode).HasMaxLength(2).IsRequired();
            entity.Property(municipality => municipality.Name).HasMaxLength(100).IsRequired();
            entity.Property(municipality => municipality.CadastralCode).HasMaxLength(4);
            entity.Property(municipality => municipality.IsProvinceCapital).IsRequired();

            entity
                .HasOne(municipality => municipality.Province)
                .WithMany(province => province.Municipalities)
                .HasForeignKey(municipality => municipality.ProvinceCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(municipality => municipality.Region)
                .WithMany(region => region.Municipalities)
                .HasForeignKey(municipality => municipality.RegionCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
