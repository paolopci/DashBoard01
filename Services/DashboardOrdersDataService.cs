using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Services;

public class DashboardOrdersDataService(DashboardOrdersDbContext dbContext) : IDashboardOrdersDataService
{
    private sealed record PagedResult<T>(List<T> Items, int CurrentPage, int PageSize, int TotalPages);

    private static readonly IReadOnlyDictionary<string, string> OrdersSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["orderNumber"] = "orderNumber",
        ["date"] = "date",
        ["amount"] = "amount",
        ["status"] = "status"
    };

    private static readonly IReadOnlyDictionary<string, string> CategorySortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["code"] = "code",
        ["name"] = "name",
        ["description"] = "description"
    };

    private static readonly IReadOnlyDictionary<string, string> CustomerSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["customer"] = "customer",
        ["ordersCount"] = "ordersCount",
        ["totalAmount"] = "totalAmount"
    };

    private static readonly IReadOnlyDictionary<string, string> ProductSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["code"] = "code",
        ["name"] = "name",
        ["category"] = "category",
        ["description"] = "description",
        ["unitCost"] = "unitCost",
        ["stock"] = "stock"
    };

    private static readonly IReadOnlyDictionary<string, string> DashboardSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["orderNumber"] = "orderNumber",
        ["customer"] = "customer",
        ["items"] = "items",
        ["date"] = "date",
        ["amount"] = "amount",
        ["status"] = "status"
    };

    public DashboardViewModel GetDashboardData(int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, DashboardSortColumns, "date");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var orders = LoadOrders();

        var filteredOrders = ApplySearch(orders, normalizedSearch, order =>
            order.OrderNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Product.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            OrderStatusPresentation.FromStatus(order.Status).Label.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        var sortedOrders = SortDashboardOrders(filteredOrders, normalizedSortBy, normalizedSortDirection);
        var pagedOrders = ApplyPaging(sortedOrders, page, pageSize);

        return new DashboardViewModel
        {
            RecentOrders = pagedOrders.Items,
            TotalOrders = sortedOrders.Count,
            TotalRevenue = sortedOrders.Where(order => order.Status != OrderStatus.Cancelled).Sum(order => order.TotalAmount),
            PendingOrders = sortedOrders.Count(order => order.Status is OrderStatus.Pending or OrderStatus.Processing),
            DeliveredOrders = sortedOrders.Count(order => order.Status == OrderStatus.Delivered),
            ActiveCustomers = dbContext.Customers.AsNoTracking().Count(),
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = pagedOrders.CurrentPage,
            PageSize = pagedOrders.PageSize,
            TotalPages = pagedOrders.TotalPages
        };
    }

    public OrdersPageViewModel GetOrdersPageData(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, OrdersSortColumns, "date");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var orders = LoadOrders();

        var selectedCustomer = customerId.HasValue
            ? dbContext.Customers.AsNoTracking().FirstOrDefault(customer => customer.Id == customerId.Value)
            : null;

        var filteredOrders = customerId.HasValue
            ? orders.Where(order => order.Customer.Id == customerId.Value).ToList()
            : orders;

        filteredOrders = ApplySearch(filteredOrders, normalizedSearch, order =>
            order.OrderNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Items.Any(item => item.ProductName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)) ||
            OrderStatusPresentation.FromStatus(order.Status).Label.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        filteredOrders = SortOrders(filteredOrders, normalizedSortBy, normalizedSortDirection);
        var pagedOrders = ApplyPaging(filteredOrders, page, pageSize);

        return new OrdersPageViewModel
        {
            Orders = pagedOrders.Items,
            TotalOrders = filteredOrders.Count,
            TotalRevenue = filteredOrders.Where(order => order.Status != OrderStatus.Cancelled).Sum(order => order.TotalAmount),
            PendingOrders = filteredOrders.Count(order => order.Status is OrderStatus.Pending or OrderStatus.Processing),
            ShippedOrders = filteredOrders.Count(order => order.Status is OrderStatus.Shipped or OrderStatus.Delivered),
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = pagedOrders.CurrentPage,
            PageSize = pagedOrders.PageSize,
            TotalPages = pagedOrders.TotalPages,
            SelectedCustomerId = selectedCustomer?.Id,
            SelectedCustomerName = selectedCustomer?.Name ?? string.Empty
        };
    }

    public CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, CustomerSortColumns, "totalAmount");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var customers = dbContext.Customers.AsNoTracking().Include(customer => customer.Orders).ToList();

        var customerSummaries = customers
            .Select(customer => new CustomerOrdersSummaryViewModel
            {
                Customer = MapCustomer(customer),
                OrdersCount = customer.Orders.Count,
                TotalOrdersAmount = customer.Orders
                    .Where(order => ToOrderStatus(order.Status) != OrderStatus.Cancelled)
                    .Sum(order => order.TotalAmount)
            })
            .ToList();

        customerSummaries = ApplySearch(customerSummaries, normalizedSearch, summary =>
            summary.Customer.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            summary.Customer.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        customerSummaries = SortCustomerSummaries(customerSummaries, normalizedSortBy, normalizedSortDirection);
        var pagedCustomers = ApplyPaging(customerSummaries, page, pageSize);

        return new CustomersPageViewModel
        {
            Customers = pagedCustomers.Items,
            TotalCustomers = customerSummaries.Count,
            CustomersWithOrders = customerSummaries.Count(summary => summary.OrdersCount > 0),
            TotalRevenue = customerSummaries.Sum(summary => summary.TotalOrdersAmount),
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = pagedCustomers.CurrentPage,
            PageSize = pagedCustomers.PageSize,
            TotalPages = pagedCustomers.TotalPages
        };
    }

    public ProductsPageViewModel GetProductsPageData(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, ProductSortColumns, "name");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "asc");
        var normalizedSearch = NormalizeSearch(search);
        var normalizedCategoryCode = NormalizeSearch(categoryCode);

        var categories = dbContext.Categories.AsNoTracking().Select(MapCategory).OrderBy(category => category.Name).ToList();
        var products = dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Select(MapProduct)
            .ToList();

        var filteredProducts = string.IsNullOrWhiteSpace(normalizedCategoryCode)
            ? products
            : products
                .Where(product => string.Equals(product.Category.Code, normalizedCategoryCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

        filteredProducts = ApplySearch(filteredProducts, normalizedSearch, product =>
            product.Code.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            product.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            product.Description.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            product.Category.Code.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            product.Category.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            product.Category.Description.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        var sortedProducts = SortProducts(filteredProducts, normalizedSortBy, normalizedSortDirection);
        var pagedProducts = ApplyPaging(sortedProducts, page, pageSize);

        return new ProductsPageViewModel
        {
            Products = pagedProducts.Items,
            Categories = categories,
            TotalProducts = sortedProducts.Count,
            TotalCategories = categories.Count,
            TotalStock = sortedProducts.Sum(product => product.Stock),
            InventoryValue = sortedProducts.Sum(product => product.UnitCost * product.Stock),
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            SelectedCategoryCode = normalizedCategoryCode,
            CurrentPage = pagedProducts.CurrentPage,
            PageSize = pagedProducts.PageSize,
            TotalPages = pagedProducts.TotalPages
        };
    }

    public CategoryPageViewModel GetCategoryPageData(string sortBy = "code", string sortDirection = "asc")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, CategorySortColumns, "code");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "asc");
        var orderedCategories = SortCategories(
            dbContext.Categories.AsNoTracking().Select(MapCategory).ToList(),
            normalizedSortBy,
            normalizedSortDirection);

        return new CategoryPageViewModel
        {
            Categories = orderedCategories,
            TotalCategories = orderedCategories.Count,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection
        };
    }

    public Category? GetCategory(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalizedCode = NormalizeCode(code);
        var category = dbContext.Categories.AsNoTracking().FirstOrDefault(category => category.Code == normalizedCode);

        return category is null ? null : MapCategory(category);
    }

    public bool CreateCategory(Category category)
    {
        var normalizedCode = NormalizeCode(category.Code);

        if (string.IsNullOrWhiteSpace(normalizedCode) || dbContext.Categories.Any(existing => existing.Code == normalizedCode))
        {
            return false;
        }

        dbContext.Categories.Add(new CategoryEntity
        {
            Code = normalizedCode,
            Name = category.Name.Trim(),
            Description = category.Description.Trim()
        });
        dbContext.SaveChanges();

        return true;
    }

    public bool UpdateCategory(Category category)
    {
        var normalizedCode = NormalizeCode(category.Code);
        var entity = dbContext.Categories.FirstOrDefault(existing => existing.Code == normalizedCode);

        if (entity is null)
        {
            return false;
        }

        entity.Name = category.Name.Trim();
        entity.Description = category.Description.Trim();
        dbContext.SaveChanges();

        return true;
    }

    public bool DeleteCategory(string code)
    {
        var normalizedCode = NormalizeCode(code);
        var entity = dbContext.Categories.FirstOrDefault(existing => existing.Code == normalizedCode);

        if (entity is null || dbContext.Products.Any(product => product.CategoryCode == normalizedCode))
        {
            return false;
        }

        dbContext.Categories.Remove(entity);
        dbContext.SaveChanges();

        return true;
    }

    private List<Order> LoadOrders()
    {
        return dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .AsSplitQuery()
            .Select(MapOrder)
            .ToList();
    }

    private static Order MapOrder(OrderEntity entity)
    {
        return new Order
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            Customer = MapCustomer(entity.Customer),
            OrderDate = entity.CreatedAt,
            TotalAmount = entity.TotalAmount,
            Status = ToOrderStatus(entity.Status),
            Items = entity.Items
                .OrderBy(item => item.Id)
                .Select(MapOrderItem)
                .ToList()
        };
    }

    private static OrderItem MapOrderItem(OrderItemEntity entity)
    {
        return new OrderItem
        {
            ProductName = entity.Product.Name,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice
        };
    }

    private static Customer MapCustomer(CustomerEntity entity)
    {
        return new Customer
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone,
            AvatarInitials = entity.AvatarInitials
        };
    }

    private static Product MapProduct(ProductEntity entity)
    {
        return new Product
        {
            Code = entity.Code,
            Name = entity.Name,
            Category = MapCategory(entity.Category),
            Description = entity.Description ?? string.Empty,
            UnitCost = entity.Price,
            Stock = entity.StockQuantity
        };
    }

    private static Category MapCategory(CategoryEntity entity)
    {
        return new Category
        {
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    private static string NormalizeSortBy(string? sortBy, IReadOnlyDictionary<string, string> allowedColumns, string defaultSortBy)
    {
        return !string.IsNullOrWhiteSpace(sortBy) && allowedColumns.TryGetValue(sortBy, out var normalizedSortBy)
            ? normalizedSortBy
            : defaultSortBy;
    }

    private static string NormalizeSortDirection(string? sortDirection, string defaultSortDirection)
    {
        if (string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase))
        {
            return "asc";
        }

        return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : defaultSortDirection;
    }

    private static string NormalizeSearch(string? search)
    {
        return (search ?? string.Empty).Trim();
    }

    private static string NormalizeCode(string? code)
    {
        return (code ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static PagedResult<T> ApplyPaging<T>(IReadOnlyList<T> items, int page, int pageSize)
    {
        var normalizedPageSize = pageSize is 10 or 20 or 50 ? pageSize : 0;
        var totalPages = normalizedPageSize == 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(items.Count / (double)normalizedPageSize));
        var normalizedPage = Math.Clamp(page, 1, totalPages);
        var pagedItems = normalizedPageSize == 0
            ? items.ToList()
            : items
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

        return new PagedResult<T>(pagedItems, normalizedPage, normalizedPageSize, totalPages);
    }

    private static List<T> ApplySearch<T>(IEnumerable<T> items, string search, Func<T, bool> predicate)
    {
        return search.Length >= 3
            ? items.Where(predicate).ToList()
            : items.ToList();
    }

    private static OrderStatus ToOrderStatus(int status)
    {
        return Enum.IsDefined(typeof(OrderStatus), status)
            ? (OrderStatus)status
            : OrderStatus.Pending;
    }

    private static List<Order> SortOrders(IEnumerable<Order> orders, string sortBy, string sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            ("orderNumber", "asc") => orders.OrderBy(order => order.OrderNumber).ThenByDescending(order => order.OrderDate).ToList(),
            ("orderNumber", "desc") => orders.OrderByDescending(order => order.OrderNumber).ThenByDescending(order => order.OrderDate).ToList(),
            ("date", "asc") => orders.OrderBy(order => order.OrderDate).ThenBy(order => order.OrderNumber).ToList(),
            ("date", "desc") => orders.OrderByDescending(order => order.OrderDate).ThenBy(order => order.OrderNumber).ToList(),
            ("amount", "asc") => orders.OrderBy(order => order.TotalAmount).ThenByDescending(order => order.OrderDate).ToList(),
            ("amount", "desc") => orders.OrderByDescending(order => order.TotalAmount).ThenByDescending(order => order.OrderDate).ToList(),
            ("status", "asc") => orders.OrderBy(order => order.Status).ThenByDescending(order => order.OrderDate).ToList(),
            ("status", "desc") => orders.OrderByDescending(order => order.Status).ThenByDescending(order => order.OrderDate).ToList(),
            _ => orders.OrderByDescending(order => order.OrderDate).ThenBy(order => order.OrderNumber).ToList()
        };
    }

    private static List<Category> SortCategories(IEnumerable<Category> categories, string sortBy, string sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            ("code", "asc") => categories.OrderBy(category => category.Code).ToList(),
            ("code", "desc") => categories.OrderByDescending(category => category.Code).ToList(),
            ("name", "asc") => categories.OrderBy(category => category.Name).ThenBy(category => category.Code).ToList(),
            ("name", "desc") => categories.OrderByDescending(category => category.Name).ThenBy(category => category.Code).ToList(),
            ("description", "asc") => categories.OrderBy(category => category.Description).ThenBy(category => category.Code).ToList(),
            ("description", "desc") => categories.OrderByDescending(category => category.Description).ThenBy(category => category.Code).ToList(),
            _ => categories.OrderBy(category => category.Code).ToList()
        };
    }

    private static List<CustomerOrdersSummaryViewModel> SortCustomerSummaries(IEnumerable<CustomerOrdersSummaryViewModel> customerSummaries, string sortBy, string sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            ("customer", "asc") => customerSummaries.OrderBy(summary => summary.Customer.Name).ThenByDescending(summary => summary.TotalOrdersAmount).ToList(),
            ("customer", "desc") => customerSummaries.OrderByDescending(summary => summary.Customer.Name).ThenByDescending(summary => summary.TotalOrdersAmount).ToList(),
            ("ordersCount", "asc") => customerSummaries.OrderBy(summary => summary.OrdersCount).ThenBy(summary => summary.Customer.Name).ToList(),
            ("ordersCount", "desc") => customerSummaries.OrderByDescending(summary => summary.OrdersCount).ThenBy(summary => summary.Customer.Name).ToList(),
            ("totalAmount", "asc") => customerSummaries.OrderBy(summary => summary.TotalOrdersAmount).ThenBy(summary => summary.Customer.Name).ToList(),
            _ => customerSummaries.OrderByDescending(summary => summary.TotalOrdersAmount).ThenBy(summary => summary.Customer.Name).ToList()
        };
    }

    private static List<Product> SortProducts(IEnumerable<Product> products, string sortBy, string sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            ("code", "asc") => products.OrderBy(product => product.Code).ToList(),
            ("code", "desc") => products.OrderByDescending(product => product.Code).ToList(),
            ("name", "asc") => products.OrderBy(product => product.Name).ThenBy(product => product.Code).ToList(),
            ("name", "desc") => products.OrderByDescending(product => product.Name).ThenBy(product => product.Code).ToList(),
            ("category", "asc") => products.OrderBy(product => product.Category.Name).ThenBy(product => product.Name).ToList(),
            ("category", "desc") => products.OrderByDescending(product => product.Category.Name).ThenBy(product => product.Name).ToList(),
            ("description", "asc") => products.OrderBy(product => product.Description).ThenBy(product => product.Name).ToList(),
            ("description", "desc") => products.OrderByDescending(product => product.Description).ThenBy(product => product.Name).ToList(),
            ("unitCost", "asc") => products.OrderBy(product => product.UnitCost).ThenBy(product => product.Name).ToList(),
            ("unitCost", "desc") => products.OrderByDescending(product => product.UnitCost).ThenBy(product => product.Name).ToList(),
            ("stock", "asc") => products.OrderBy(product => product.Stock).ThenBy(product => product.Name).ToList(),
            ("stock", "desc") => products.OrderByDescending(product => product.Stock).ThenBy(product => product.Name).ToList(),
            _ => products.OrderBy(product => product.Code).ToList()
        };
    }

    private static List<Order> SortDashboardOrders(IEnumerable<Order> orders, string sortBy, string sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            ("orderNumber", "asc") => orders.OrderBy(order => order.OrderNumber).ToList(),
            ("orderNumber", "desc") => orders.OrderByDescending(order => order.OrderNumber).ToList(),
            ("customer", "asc") => orders.OrderBy(order => order.Customer.Name).ThenByDescending(order => order.OrderDate).ToList(),
            ("customer", "desc") => orders.OrderByDescending(order => order.Customer.Name).ThenByDescending(order => order.OrderDate).ToList(),
            ("items", "asc") => orders.OrderBy(order => order.ItemsCount).ThenBy(order => order.Quantity).ThenByDescending(order => order.OrderDate).ToList(),
            ("items", "desc") => orders.OrderByDescending(order => order.ItemsCount).ThenByDescending(order => order.Quantity).ThenByDescending(order => order.OrderDate).ToList(),
            ("date", "asc") => orders.OrderBy(order => order.OrderDate).ThenBy(order => order.OrderNumber).ToList(),
            ("date", "desc") => orders.OrderByDescending(order => order.OrderDate).ThenBy(order => order.OrderNumber).ToList(),
            ("amount", "asc") => orders.OrderBy(order => order.TotalAmount).ThenByDescending(order => order.OrderDate).ToList(),
            ("amount", "desc") => orders.OrderByDescending(order => order.TotalAmount).ThenByDescending(order => order.OrderDate).ToList(),
            ("status", "asc") => orders.OrderBy(order => order.Status).ThenByDescending(order => order.OrderDate).ToList(),
            ("status", "desc") => orders.OrderByDescending(order => order.Status).ThenByDescending(order => order.OrderDate).ToList(),
            _ => orders.OrderByDescending(order => order.OrderDate).ThenBy(order => order.OrderNumber).ToList()
        };
    }
}
