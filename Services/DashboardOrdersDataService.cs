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

        public OrdersPageViewModel GetOrdersPageData(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "")
    {
        var normalizedDateFrom = NormalizeDateFilter(dateFrom);
        var normalizedDateTo = NormalizeDateFilter(dateTo);
        var paginationResult = GetOrders(customerId, page, pageSize, sortBy, sortDirection, search, normalizedDateFrom, normalizedDateTo);
        var selectedCustomer = customerId.HasValue
            ? dbContext.Customers.AsNoTracking().FirstOrDefault(customer => customer.Id == customerId.Value)
            : null;

        return new OrdersPageViewModel
        {
            Orders = paginationResult.Items,
            TotalOrders = paginationResult.TotalItems,
            TotalRevenue = paginationResult.Items.Where(order => order.Status != OrderStatus.Cancelled).Sum(order => order.TotalAmount),
            PendingOrders = paginationResult.Items.Count(order => order.Status is OrderStatus.Pending or OrderStatus.Processing),
            ShippedOrders = paginationResult.Items.Count(order => order.Status is OrderStatus.Shipped or OrderStatus.Delivered),
            SearchTerm = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            CurrentPage = paginationResult.CurrentPage,
            PageSize = paginationResult.PageSize,
            TotalPages = paginationResult.TotalPages,
            SelectedCustomerId = selectedCustomer?.Id,
            SelectedCustomerName = selectedCustomer?.Name ?? string.Empty
        };
    }

    public PaginationResult<Order> GetOrders(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, OrdersSortColumns, "date");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var normalizedDateFrom = NormalizeDateFilter(dateFrom);
        var normalizedDateTo = NormalizeDateFilter(dateTo);
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
        filteredOrders = ApplyOrderDateFilter(filteredOrders, normalizedDateFrom, normalizedDateTo);

        var totalCount = filteredOrders.Count;
        filteredOrders = SortOrders(filteredOrders, normalizedSortBy, normalizedSortDirection);
        var pagedOrders = ApplyPagingImproved(filteredOrders, page, pageSize);

        return new PaginationResult<Order>(
            pagedOrders.Items,
            pagedOrders.CurrentPage,
            pagedOrders.PageSize,
            totalCount,
            pagedOrders.TotalPages);
    }

    public OrdersPageViewModel GetOrdersPageDataForCustomerEmail(string? email, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "")
    {
        var customer = GetCustomerByEmail(email);
        if (customer is null)
        {
            return CreateEmptyOrdersPage(page, pageSize, sortBy, sortDirection, search, dateFrom, dateTo);
        }

        return GetOrdersPageData(customer.Id, page, pageSize, sortBy, sortDirection, search, dateFrom, dateTo);
    }

        public CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
    {
        var paginationResult = GetCustomers(page, pageSize, sortBy, sortDirection, search);

        return new CustomersPageViewModel
        {
            Customers = paginationResult.Items,
            TotalCustomers = paginationResult.TotalItems,
            CustomersWithOrders = paginationResult.Items.Count(summary => summary.OrdersCount > 0),
            TotalRevenue = paginationResult.Items.Sum(summary => summary.TotalOrdersAmount),
            SearchTerm = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            CurrentPage = paginationResult.CurrentPage,
            PageSize = paginationResult.PageSize,
            TotalPages = paginationResult.TotalPages
        };
    }

    public PaginationResult<CustomerOrdersSummaryViewModel> GetCustomers(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
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

        var totalCount = customerSummaries.Count;
        customerSummaries = SortCustomerSummaries(customerSummaries, normalizedSortBy, normalizedSortDirection);
        var pagedCustomers = ApplyPagingImproved(customerSummaries, page, pageSize);

        return new PaginationResult<CustomerOrdersSummaryViewModel>(
            pagedCustomers.Items,
            pagedCustomers.CurrentPage,
            pagedCustomers.PageSize,
            totalCount,
            pagedCustomers.TotalPages);
    }

        public ProductsPageViewModel GetProductsPageData(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "")
    {
        var categories = dbContext.Categories.AsNoTracking().Select(MapCategory).OrderBy(category => category.Name).ToList();
        var paginationResult = GetProducts(page, pageSize, sortBy, sortDirection, categoryCode, search);

        return new ProductsPageViewModel
        {
            Products = paginationResult.Items,
            Categories = categories,
            TotalProducts = paginationResult.TotalItems,
            TotalCategories = categories.Count,
            TotalStock = paginationResult.Items.Sum(product => product.Stock),
            InventoryValue = paginationResult.Items.Sum(product => product.UnitCost * product.Stock),
            SearchTerm = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            SelectedCategoryCode = categoryCode,
            CurrentPage = paginationResult.CurrentPage,
            PageSize = paginationResult.PageSize,
            TotalPages = paginationResult.TotalPages
        };
    }

    public PaginationResult<Product> GetProducts(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "")
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

        var totalCount = filteredProducts.Count;
        var sortedProducts = SortProducts(filteredProducts, normalizedSortBy, normalizedSortDirection);
        var pagedProducts = ApplyPagingImproved(sortedProducts, page, pageSize);

        return new PaginationResult<Product>(
            pagedProducts.Items,
            pagedProducts.CurrentPage,
            pagedProducts.PageSize,
            totalCount,
            pagedProducts.TotalPages);
    }

    public Customer? GetCustomer(int id)
    {
        var customer = dbContext.Customers.AsNoTracking().FirstOrDefault(customer => customer.Id == id);

        return customer is null ? null : MapCustomer(customer);
    }

    public Customer? GetCustomerByEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var customer = dbContext.Customers
            .AsNoTracking()
            .FirstOrDefault(customer => customer.Email.ToLower() == normalizedEmail);

        return customer is null ? null : MapCustomer(customer);
    }

    public bool UpdateCustomer(Customer customer)
    {
        var entity = dbContext.Customers.FirstOrDefault(existing => existing.Id == customer.Id);
        if (entity is null)
        {
            return false;
        }

        entity.Name = customer.Name.Trim();
        entity.Email = customer.Email.Trim().ToLowerInvariant();
        entity.Phone = customer.Phone.Trim();
        entity.AvatarInitials = CreateInitials(entity.Name);
        dbContext.SaveChanges();

        return true;
    }

    public Product? GetProduct(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalizedCode = NormalizeCode(code);
        var product = dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefault(product => product.Code == normalizedCode);

        return product is null ? null : MapProduct(product);
    }

    public List<Product> GetAvailableProducts()
    {
        return dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.StockQuantity > 0)
            .OrderBy(product => product.Name)
            .Select(MapProduct)
            .ToList();
    }

    public bool CreateProduct(Product product)
    {
        var normalizedCode = NormalizeCode(product.Code);
        if (string.IsNullOrWhiteSpace(normalizedCode) || dbContext.Products.Any(existing => existing.Code == normalizedCode))
        {
            return false;
        }

        var categoryCode = NormalizeCode(product.Category.Code);
        if (!dbContext.Categories.Any(category => category.Code == categoryCode))
        {
            return false;
        }

        dbContext.Products.Add(new ProductEntity
        {
            Code = normalizedCode,
            Name = product.Name.Trim(),
            Description = product.Description.Trim(),
            Price = product.UnitCost,
            StockQuantity = product.Stock,
            CategoryCode = categoryCode,
            ImageUrl = NormalizeImageUrl(product.ImageUrl),
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();

        return true;
    }

    public bool UpdateProduct(Product product)
    {
        var normalizedCode = NormalizeCode(product.Code);
        var categoryCode = NormalizeCode(product.Category.Code);
        var entity = dbContext.Products.FirstOrDefault(existing => existing.Code == normalizedCode);
        if (entity is null || !dbContext.Categories.Any(category => category.Code == categoryCode))
        {
            return false;
        }

        entity.Name = product.Name.Trim();
        entity.Description = product.Description.Trim();
        entity.Price = product.UnitCost;
        entity.StockQuantity = product.Stock;
        entity.CategoryCode = categoryCode;
        entity.ImageUrl = NormalizeImageUrl(product.ImageUrl);
        dbContext.SaveChanges();

        return true;
    }

    public bool CreateOrder(string? customerEmail, string productCode, int quantity)
    {
        return CreateOrder(customerEmail, [new NewOrderItemViewModel { ProductCode = productCode, Quantity = quantity }]);
    }

    public bool CreateOrder(string? customerEmail, IReadOnlyList<NewOrderItemViewModel> items)
    {
        if (string.IsNullOrWhiteSpace(customerEmail) || items.Count == 0)
        {
            return false;
        }

        var requestedItems = items
            .Select(item => new
            {
                ProductCode = NormalizeCode(item.ProductCode),
                item.Quantity
            })
            .ToList();

        if (requestedItems.Any(item => string.IsNullOrWhiteSpace(item.ProductCode) || item.Quantity <= 0))
        {
            return false;
        }

        if (requestedItems.Select(item => item.ProductCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() != requestedItems.Count)
        {
            return false;
        }

        var requestedCodes = requestedItems.Select(item => item.ProductCode).ToList();
        var products = dbContext.Products
            .Where(product => requestedCodes.Contains(product.Code))
            .ToList();

        if (products.Count != requestedItems.Count)
        {
            return false;
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products.Single(product => product.Code == requestedItem.ProductCode);
            if (product.StockQuantity <= 0 || product.StockQuantity < requestedItem.Quantity)
            {
                return false;
            }
        }

        var customer = FindOrCreateCustomer(customerEmail);
        var order = new OrderEntity
        {
            OrderNumber = CreateNextOrderNumber(),
            CustomerId = customer.Id,
            TotalAmount = requestedItems.Sum(item =>
            {
                var product = products.Single(product => product.Code == item.ProductCode);
                return product.Price * item.Quantity;
            }),
            Status = (int)OrderStatus.Pending,
            CreatedAt = DateTime.Now
        };

        foreach (var requestedItem in requestedItems)
        {
            var product = products.Single(product => product.Code == requestedItem.ProductCode);
            product.StockQuantity -= requestedItem.Quantity;
            order.Items.Add(new OrderItemEntity
            {
                ProductId = product.Id,
                Quantity = requestedItem.Quantity,
                UnitPrice = product.Price
            });
        }

        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        return true;
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

    private OrdersPageViewModel CreateEmptyOrdersPage(int page, int pageSize, string sortBy, string sortDirection, string search, string dateFrom, string dateTo)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, OrdersSortColumns, "date");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var normalizedDateFrom = NormalizeDateFilter(dateFrom);
        var normalizedDateTo = NormalizeDateFilter(dateTo);
        var pagedOrders = ApplyPaging(Array.Empty<Order>(), page, pageSize);

        return new OrdersPageViewModel
        {
            Orders = [],
            TotalOrders = 0,
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            CurrentPage = pagedOrders.CurrentPage,
            PageSize = pagedOrders.PageSize,
            TotalPages = pagedOrders.TotalPages
        };
    }

    private CustomerEntity FindOrCreateCustomer(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = dbContext.Customers.FirstOrDefault(customer => customer.Email.ToLower() == normalizedEmail);
        if (existing is not null)
        {
            return existing;
        }

        var displayName = normalizedEmail.Split('@')[0].Replace('.', ' ').Replace('_', ' ').Replace('-', ' ');
        displayName = string.IsNullOrWhiteSpace(displayName) ? normalizedEmail : displayName;
        var customer = new CustomerEntity
        {
            Name = displayName,
            Email = normalizedEmail,
            Phone = "-",
            AvatarInitials = CreateInitials(displayName),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        dbContext.SaveChanges();

        return customer;
    }

    private string CreateNextOrderNumber()
    {
        var nextId = dbContext.Orders.Any()
            ? dbContext.Orders.Max(order => order.Id) + 1
            : 1;

        return $"ORD-{DateTime.Now.Year}-{nextId:D3}";
    }

    private static string CreateInitials(string name)
    {
        var initials = string.Join(
            string.Empty,
            name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => char.ToUpperInvariant(part[0])));

        return string.IsNullOrWhiteSpace(initials) ? "UT" : initials;
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
            ImageUrl = entity.ImageUrl ?? string.Empty,
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

    private static string NormalizeDateFilter(string? date)
    {
        var normalizedDate = (date ?? string.Empty).Trim();
        return DateTime.TryParse(normalizedDate, out var parsedDate)
            ? parsedDate.ToString("yyyy-MM-dd")
            : string.Empty;
    }

    private static string NormalizeCode(string? code)
    {
        return (code ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string? NormalizeImageUrl(string? imageUrl)
    {
        var normalizedImageUrl = (imageUrl ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalizedImageUrl) ? null : normalizedImageUrl;
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

    private static PagedResult<T> ApplyPagingImproved<T>(IReadOnlyList<T> items, int page, int pageSize)
    {
        // Standard page sizes
        var standardPageSizes = new[] { 10, 25, 50, 100 };
        var normalizedPageSize = pageSize == 0
            ? 0
            : standardPageSizes.Contains(pageSize) ? pageSize : 10;
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

    private static List<Order> ApplyOrderDateFilter(IEnumerable<Order> orders, string dateFrom, string dateTo)
    {
        var hasDateFrom = DateTime.TryParse(dateFrom, out var from);
        var hasDateTo = DateTime.TryParse(dateTo, out var to);

        if (!hasDateFrom && !hasDateTo)
        {
            return orders.ToList();
        }

        return orders
            .Where(order => !hasDateFrom || order.OrderDate.Date >= from.Date)
            .Where(order => !hasDateTo || order.OrderDate.Date <= to.Date)
            .ToList();
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
