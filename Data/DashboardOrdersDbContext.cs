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
    }
}
