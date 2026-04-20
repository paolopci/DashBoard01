using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Services;

public class DashboardOrdersDatabaseSeeder(DashboardOrdersDbContext dbContext, UserManager<ApplicationUser> userManager)
{
    private const string DefaultCustomerPassword = "Micene@65";
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var categories = MockDataService.GetCategories();
        var products = MockDataService.GetProducts();
        var customers = MockDataService.GetCustomers();
        var orders = MockDataService.GetOrders();

        await SeedCategoriesAsync(categories, cancellationToken);
        await SeedProductsAsync(products, cancellationToken);
        var customerIdsByEmail = await SeedCustomersAsync(customers, cancellationToken);
        await SeedCustomerUsersAsync(cancellationToken);
        var productIdsByName = await GetProductIdsByNameAsync(products, cancellationToken);
        await SeedOrdersAsync(orders, customerIdsByEmail, productIdsByName, cancellationToken);
    }

    private async Task SeedCategoriesAsync(IEnumerable<Models.Category> categories, CancellationToken cancellationToken)
    {
        var existingCategories = await dbContext.Categories
            .ToDictionaryAsync(category => category.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var category in categories)
        {
            if (existingCategories.TryGetValue(category.Code, out var existingCategory))
            {
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                continue;
            }

            dbContext.Categories.Add(new CategoryEntity
            {
                Code = category.Code,
                Name = category.Name,
                Description = category.Description
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedProductsAsync(IEnumerable<Models.Product> products, CancellationToken cancellationToken)
    {
        var existingProducts = await dbContext.Products
            .ToDictionaryAsync(product => product.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var product in products)
        {
            if (existingProducts.TryGetValue(product.Code, out var existingProduct))
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.UnitCost;
                existingProduct.StockQuantity = product.Stock;
                existingProduct.CategoryCode = product.Category.Code;
                continue;
            }

            dbContext.Products.Add(new ProductEntity
            {
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Price = product.UnitCost,
                StockQuantity = product.Stock,
                CategoryCode = product.Category.Code,
                CreatedAt = SeedCreatedAt
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, int>> SeedCustomersAsync(IEnumerable<Models.Customer> customers, CancellationToken cancellationToken)
    {
        var customerEmails = customers
            .Select(customer => customer.Email)
            .ToList();
        var existingCustomers = await dbContext.Customers
            .ToDictionaryAsync(customer => customer.Email, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var customer in customers)
        {
            if (existingCustomers.TryGetValue(customer.Email, out var existingCustomer))
            {
                existingCustomer.Name = customer.Name;
                existingCustomer.Phone = customer.Phone;
                existingCustomer.AvatarInitials = customer.AvatarInitials;
                continue;
            }

            dbContext.Customers.Add(new CustomerEntity
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                AvatarInitials = customer.AvatarInitials,
                CreatedAt = SeedCreatedAt
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Customers
            .Where(customer => customerEmails.Contains(customer.Email))
            .ToDictionaryAsync(customer => customer.Email, customer => customer.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    private async Task SeedCustomerUsersAsync(CancellationToken cancellationToken)
    {
        var customers = await dbContext.Customers
            .OrderBy(customer => customer.Id)
            .ToListAsync(cancellationToken);

        foreach (var customer in customers)
        {
            var normalizedEmail = customer.Email.Trim().ToLowerInvariant();
            var user = await userManager.FindByEmailAsync(normalizedEmail);

            if (user is null)
            {
                user = CreateApplicationUser(customer, normalizedEmail);
                var createResult = await userManager.CreateAsync(user, DefaultCustomerPassword);
                EnsureIdentityResultSucceeded(createResult, $"Creazione utente cliente {normalizedEmail} non completata.");
                continue;
            }

            var hasPassword = await userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                var removeResult = await userManager.RemovePasswordAsync(user);
                EnsureIdentityResultSucceeded(removeResult, $"Reset password cliente {normalizedEmail} non completato.");
            }

            var addPasswordResult = await userManager.AddPasswordAsync(user, DefaultCustomerPassword);
            EnsureIdentityResultSucceeded(addPasswordResult, $"Impostazione password cliente {normalizedEmail} non completata.");
        }
    }

    private static ApplicationUser CreateApplicationUser(CustomerEntity customer, string normalizedEmail)
    {
        var (firstName, lastName) = SplitCustomerName(customer.Name);

        return new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = new DateTime(1990, 1, 1),
            City = "Non indicata",
            Country = "Italia",
            FiscalCode = CreateSeedFiscalCode(customer.Id)
        };
    }

    private static (string FirstName, string LastName) SplitCustomerName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return ("Cliente", "Dashboard");
        }

        if (parts.Length == 1)
        {
            return (parts[0], "Dashboard");
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static string CreateSeedFiscalCode(int customerId)
    {
        return $"CUST{customerId:D12}";
    }

    private static void EnsureIdentityResultSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }

    private async Task<Dictionary<string, int>> GetProductIdsByNameAsync(IEnumerable<Models.Product> products, CancellationToken cancellationToken)
    {
        var productCodes = products
            .Select(product => product.Code)
            .ToList();

        return await dbContext.Products
            .Where(product => productCodes.Contains(product.Code))
            .ToDictionaryAsync(product => product.Name, product => product.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    private async Task SeedOrdersAsync(
        IEnumerable<Models.Order> orders,
        IReadOnlyDictionary<string, int> customerIdsByEmail,
        IReadOnlyDictionary<string, int> productIdsByName,
        CancellationToken cancellationToken)
    {
        var existingOrderNumbers = await dbContext.Orders
            .Select(order => order.OrderNumber)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var order in orders)
        {
            if (existingOrderNumbers.Contains(order.OrderNumber))
            {
                continue;
            }

            if (!customerIdsByEmail.TryGetValue(order.Customer.Email, out var customerId))
            {
                continue;
            }

            var orderEntity = new OrderEntity
            {
                OrderNumber = order.OrderNumber,
                CustomerId = customerId,
                TotalAmount = order.TotalAmount,
                Status = (int)order.Status,
                CreatedAt = order.OrderDate
            };

            foreach (var item in order.Items)
            {
                if (!productIdsByName.TryGetValue(item.ProductName, out var productId))
                {
                    continue;
                }

                orderEntity.Items.Add(new OrderItemEntity
                {
                    ProductId = productId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            dbContext.Orders.Add(orderEntity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
