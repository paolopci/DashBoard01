using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Services;

public class DashboardOrdersDataService(DashboardOrdersDbContext dbContext) : IDashboardOrdersDataService
{
    private const int CartRetentionDays = 30;
    private const int CheckoutRetentionHours = 24;
    private const string PaymentMethodTestCard = "test-card";
    private const string PaymentMethodPending = "pending";
    private const string PaymentMethodStripeTest = "stripe-test";

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
            TotalRevenue = sortedOrders.Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(order.Status)).Sum(order => order.TotalAmount),
            PendingOrders = sortedOrders.Count(order => OrderStatusMetricsPolicy.IsOperationallyActive(order.Status)),
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
            TotalRevenue = paginationResult.Items.Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(order.Status)).Sum(order => order.TotalAmount),
            PendingOrders = paginationResult.Items.Count(order => OrderStatusMetricsPolicy.IsOperationallyActive(order.Status)),
            ShippedOrders = paginationResult.Items.Count(order => OrderStatusMetricsPolicy.IsFulfillmentCompleted(order.Status)),
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
                    .Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(ToOrderStatus(order.Status)))
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
            .Include(product => product.CarouselImages)
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
            .Include(product => product.CarouselImages)
            .FirstOrDefault(product => product.Code == normalizedCode);

        return product is null ? null : MapProduct(product);
    }

    public List<Product> GetAvailableProducts()
    {
        return dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.CarouselImages)
            .Where(product => product.StockQuantity > 0)
            .OrderBy(product => product.Name)
            .Select(MapProduct)
            .ToList();
    }

    public CartViewModel GetCart(string? customerEmail)
    {
        var cart = GetActiveCart(customerEmail, trackChanges: false);
        return cart is null ? new CartViewModel() : MapCart(cart);
    }

    public int GetCartItemsCount(string? customerEmail)
    {
        return GetCart(customerEmail).TotalItems;
    }

    public bool AddOrUpdateCartItem(string? customerEmail, string productCode, int quantity)
    {
        if (quantity <= 0)
        {
            return false;
        }

        var normalizedEmail = NormalizeEmail(customerEmail);
        var normalizedProductCode = NormalizeCode(productCode);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(normalizedProductCode))
        {
            return false;
        }

        var product = dbContext.Products.FirstOrDefault(existing => existing.Code == normalizedProductCode);
        if (product is null || product.StockQuantity <= 0)
        {
            return false;
        }

        var now = GetCurrentTimestamp();
        var cart = GetOrCreateActiveCart(normalizedEmail, now);
        var item = cart.Items.FirstOrDefault(existing => existing.ProductId == product.Id);
        var newQuantity = (item?.Quantity ?? 0) + quantity;

        if (newQuantity > product.StockQuantity)
        {
            return false;
        }

        if (item is null)
        {
            cart.Items.Add(new CartItemEntity
            {
                ProductId = product.Id,
                Quantity = quantity,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            item.Quantity = newQuantity;
            item.UpdatedAt = now;
        }

        TouchCart(cart, now);
        dbContext.SaveChanges();
        return true;
    }

    public bool UpdateCartItemQuantity(string? customerEmail, string productCode, int quantity)
    {
        if (quantity <= 0)
        {
            return false;
        }

        var normalizedProductCode = NormalizeCode(productCode);
        var cart = GetActiveCart(customerEmail, trackChanges: true);
        if (cart is null || string.IsNullOrWhiteSpace(normalizedProductCode))
        {
            return false;
        }

        var item = cart.Items.FirstOrDefault(existing => existing.Product.Code == normalizedProductCode);
        if (item is null || quantity > item.Product.StockQuantity)
        {
            return false;
        }

        var now = GetCurrentTimestamp();
        item.Quantity = quantity;
        item.UpdatedAt = now;
        TouchCart(cart, now);
        dbContext.SaveChanges();
        return true;
    }

    public bool RemoveCartItem(string? customerEmail, string productCode)
    {
        var normalizedProductCode = NormalizeCode(productCode);
        var cart = GetActiveCart(customerEmail, trackChanges: true);
        if (cart is null || string.IsNullOrWhiteSpace(normalizedProductCode))
        {
            return false;
        }

        var item = cart.Items.FirstOrDefault(existing => existing.Product.Code == normalizedProductCode);
        if (item is null)
        {
            return false;
        }

        dbContext.CartItems.Remove(item);
        TouchCart(cart, GetCurrentTimestamp());
        dbContext.SaveChanges();
        return true;
    }

    public bool ClearCart(string? customerEmail)
    {
        var cart = GetActiveCart(customerEmail, trackChanges: true);
        if (cart is null)
        {
            return false;
        }

        dbContext.CartItems.RemoveRange(cart.Items);
        TouchCart(cart, GetCurrentTimestamp());
        dbContext.SaveChanges();
        return true;
    }

    public bool StartCheckout(string? customerEmail)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        var cart = GetCart(normalizedEmail);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || cart.Items.Count == 0 || cart.HasUnavailableItems)
        {
            return false;
        }

        var now = GetCurrentTimestamp();
        var session = GetActiveCheckoutSession(normalizedEmail, trackChanges: true);
        if (session is null)
        {
            session = new CheckoutSessionEntity
            {
                CustomerEmail = normalizedEmail,
                CreatedAt = now
            };
            dbContext.CheckoutSessions.Add(session);
        }

        session.CurrentStep = (int)CheckoutStep.Summary;
        session.TotalItems = cart.TotalItems;
        session.TotalAmount = cart.TotalAmount;
        session.CreatedOrderId = null;
        TouchCheckoutSession(session, now);
        dbContext.SaveChanges();
        return true;
    }

    public CheckoutSessionViewModel? GetCheckout(string? customerEmail)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        var session = GetActiveCheckoutSession(normalizedEmail, trackChanges: false);
        return session is null ? null : MapCheckoutSession(session, GetCart(normalizedEmail));
    }

    public List<string> GetItalianProvinces()
    {
        return dbContext.ItalianProvinces
            .AsNoTracking()
            .Select(province => province.Name)
            .Distinct()
            .OrderBy(provinceName => provinceName)
            .ToList();
    }

    public List<string> GetItalianCities(string? provinceName)
    {
        var normalizedProvinceName = NormalizeText(provinceName);
        if (string.IsNullOrWhiteSpace(normalizedProvinceName))
        {
            return [];
        }

        return dbContext.ItalianMunicipalities
            .AsNoTracking()
            .Where(municipality => municipality.Province.Name == normalizedProvinceName)
            .Select(municipality => municipality.Name)
            .Distinct()
            .OrderBy(cityName => cityName)
            .ToList();
    }

    public List<string> GetItalianPostalCodes(string? provinceName, string? cityName)
    {
        var normalizedProvinceName = NormalizeText(provinceName);
        var normalizedCityName = NormalizeText(cityName);
        if (string.IsNullOrWhiteSpace(normalizedProvinceName) || string.IsNullOrWhiteSpace(normalizedCityName))
        {
            return [];
        }

        return dbContext.ItalianPostalCodes
            .AsNoTracking()
            .Where(postalCode => postalCode.ProvinceName == normalizedProvinceName && postalCode.CityName == normalizedCityName)
            .Select(postalCode => postalCode.PostalCode)
            .Distinct()
            .OrderBy(postalCode => postalCode)
            .ToList();
    }

    public bool SaveCheckoutAddresses(string? customerEmail, CheckoutAddressesViewModel model)
    {
        var session = GetActiveCheckoutSession(customerEmail, trackChanges: true);
        if (session is null || !AreShippingFieldsValid(model) || !AreBillingFieldsValid(model))
        {
            return false;
        }

        session.ShippingFullName = ResolveShippingFullName(model);
        session.ShippingAddressLine = ResolveShippingAddressLine(model);
        session.ShippingCity = NormalizeText(model.ShippingCity);
        session.ShippingPostalCode = NormalizeText(model.ShippingPostalCode);
        session.ShippingCountry = ResolveShippingCountry(model);
        session.ShippingPhone = ResolveShippingPhone(model);
        session.BillingSameAsShipping = model.BillingSameAsShipping;
        session.BillingFullName = model.BillingSameAsShipping ? session.ShippingFullName : ResolveBillingFullName(model);
        session.BillingAddressLine = model.BillingSameAsShipping ? session.ShippingAddressLine : ResolveBillingAddressLine(model);
        session.BillingCity = model.BillingSameAsShipping ? session.ShippingCity : NormalizeText(model.BillingCity);
        session.BillingPostalCode = model.BillingSameAsShipping ? session.ShippingPostalCode : NormalizeText(model.BillingPostalCode);
        session.BillingCountry = model.BillingSameAsShipping ? session.ShippingCountry : ResolveBillingCountry(model);
        session.BillingVatNumber = NormalizeOptionalText(model.BillingVatNumber);
        session.CurrentStep = (int)CheckoutStep.Addresses;
        TouchCheckoutSession(session, GetCurrentTimestamp());
        dbContext.SaveChanges();
        return true;
    }

    public bool SaveCheckoutOptions(string? customerEmail, CheckoutOptionsViewModel model)
    {
        var session = GetActiveCheckoutSession(customerEmail, trackChanges: true);
        if (session is null || !HasCheckoutAddresses(session))
        {
            return false;
        }

        var deliveryMethod = NormalizeCheckoutChoice(model.DeliveryMethod, "standard");
        var paymentMethod = NormalizeCheckoutChoice(model.PaymentMethod, PaymentMethodPending);
        if (paymentMethod is not PaymentMethodTestCard and not PaymentMethodPending and not PaymentMethodStripeTest)
        {
            return false;
        }

        session.DeliveryMethod = deliveryMethod;
        session.PaymentMethod = paymentMethod;
        session.CurrentStep = (int)CheckoutStep.Confirm;
        TouchCheckoutSession(session, GetCurrentTimestamp());
        dbContext.SaveChanges();
        return true;
    }

    public CheckoutConfirmResult ConfirmCheckout(string? customerEmail)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        var session = GetActiveCheckoutSession(normalizedEmail, trackChanges: true);
        if (session is null || !HasCheckoutAddresses(session))
        {
            return CheckoutConfirmResult.Failed("Checkout non valido o scaduto.");
        }

        var cart = GetCart(normalizedEmail);
        if (cart.Items.Count == 0)
        {
            return CheckoutConfirmResult.Failed("Il carrello e vuoto.");
        }

        if (cart.HasUnavailableItems)
        {
            return CheckoutConfirmResult.Failed("Uno o piu articoli non sono disponibili.");
        }

        var items = cart.Items
            .Select(item => new NewOrderItemViewModel { ProductCode = item.ProductCode, Quantity = item.Quantity })
            .ToList();
        var requiresPayment = session.PaymentMethod is PaymentMethodTestCard or PaymentMethodStripeTest;
        var initialStatus = requiresPayment ? OrderStatus.PaymentPending : OrderStatus.Pending;
        var order = CreateOrderEntity(normalizedEmail, items, initialStatus);
        if (order is null)
        {
            return CheckoutConfirmResult.Failed("Ordine non creato. Verifica disponibilita articoli.");
        }

        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        dbContext.OrderCheckoutDetails.Add(CreateOrderCheckoutDetails(order.Id, session, initialStatus));
        session.CreatedOrderId = order.Id;
        session.CurrentStep = requiresPayment
            ? (int)CheckoutStep.Payment
            : (int)CheckoutStep.Result;
        TouchCheckoutSession(session, GetCurrentTimestamp());
        ClearCart(normalizedEmail);
        dbContext.SaveChanges();

        return new CheckoutConfirmResult
        {
            Success = true,
            OrderId = order.Id,
            RequiresPayment = requiresPayment,
            RequiresStripeCheckout = session.PaymentMethod == PaymentMethodStripeTest,
            PaymentMethod = session.PaymentMethod
        };
    }

    public bool SaveStripeCheckoutSession(string? customerEmail, int orderId, StripeCheckoutSessionResult session)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        if (string.IsNullOrWhiteSpace(session.SessionId) || string.IsNullOrWhiteSpace(session.Url))
        {
            return false;
        }

        var order = dbContext.Orders
            .Include(existingOrder => existingOrder.Customer)
            .Include(existingOrder => existingOrder.CheckoutDetails)
            .FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order is null
            || order.CheckoutDetails is null
            || order.Customer.Email.ToLowerInvariant() != normalizedEmail
            || order.CheckoutDetails.PaymentMethod != PaymentMethodStripeTest)
        {
            return false;
        }

        order.CheckoutDetails.StripeCheckoutSessionId = NormalizeOptionalText(session.SessionId);
        order.CheckoutDetails.StripePaymentIntentId = NormalizeOptionalText(session.PaymentIntentId);
        order.CheckoutDetails.StripePaymentStatus = NormalizeOptionalText(session.PaymentStatus);
        order.CheckoutDetails.PaymentStatus = "pending";
        order.CheckoutDetails.UpdatedAt = GetCurrentTimestamp();
        dbContext.SaveChanges();
        return true;
    }

    public int? GetOrderIdByStripeCheckoutSession(string stripeCheckoutSessionId)
    {
        var normalizedSessionId = NormalizeOptionalText(stripeCheckoutSessionId);
        if (string.IsNullOrWhiteSpace(normalizedSessionId))
        {
            return null;
        }

        return dbContext.OrderCheckoutDetails
            .AsNoTracking()
            .Where(details => details.StripeCheckoutSessionId == normalizedSessionId)
            .Select(details => (int?)details.OrderId)
            .FirstOrDefault();
    }

    public CheckoutPaymentResult CompleteStripePayment(string stripeCheckoutSessionId, string? paymentIntentId, string? stripePaymentStatus, string? changedBy = null)
    {
        var order = GetStripeOrderForUpdate(stripeCheckoutSessionId);
        if (order is null || order.CheckoutDetails is null)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Sessione Stripe non trovata." };
        }

        if (order.CheckoutDetails.PaymentMethod != PaymentMethodStripeTest)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Pagamento Stripe non disponibile per questo ordine." };
        }

        var currentStatus = ToOrderStatus(order.Status);
        if (currentStatus == OrderStatus.Confirmed)
        {
            UpdateStripeReferences(order.CheckoutDetails, paymentIntentId, stripePaymentStatus, "authorized");
            dbContext.SaveChanges();
            return new CheckoutPaymentResult
            {
                Success = true,
                OrderId = order.Id,
                FinalStatus = OrderStatus.Confirmed,
                PaymentStatus = order.CheckoutDetails.PaymentStatus
            };
        }

        if (currentStatus != OrderStatus.PaymentPending && currentStatus != OrderStatus.PaymentAuthorized)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Stato ordine non compatibile con conferma Stripe." };
        }

        var now = GetCurrentTimestamp();
        var actor = string.IsNullOrWhiteSpace(changedBy) ? "stripe" : changedBy;
        if (currentStatus == OrderStatus.PaymentPending)
        {
            AppendStatusHistory(order, currentStatus, OrderStatus.PaymentAuthorized, actor, "Pagamento Stripe test autorizzato", stripeCheckoutSessionId, now);
            order.Status = (int)OrderStatus.PaymentAuthorized;
        }

        AppendStatusHistory(order, OrderStatus.PaymentAuthorized, OrderStatus.Confirmed, actor, "Ordine confermato dopo pagamento Stripe test", stripeCheckoutSessionId, now);
        order.Status = (int)OrderStatus.Confirmed;
        order.UpdatedAt = now;
        UpdateStripeReferences(order.CheckoutDetails, paymentIntentId, stripePaymentStatus, "authorized");
        order.CheckoutDetails.UpdatedAt = now;

        dbContext.SaveChanges();
        return new CheckoutPaymentResult
        {
            Success = true,
            OrderId = order.Id,
            FinalStatus = OrderStatus.Confirmed,
            PaymentStatus = order.CheckoutDetails.PaymentStatus
        };
    }

    public CheckoutPaymentResult FailStripePayment(string stripeCheckoutSessionId, string? paymentIntentId, string? stripePaymentStatus, string? changedBy = null)
    {
        var order = GetStripeOrderForUpdate(stripeCheckoutSessionId);
        if (order is null || order.CheckoutDetails is null)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Sessione Stripe non trovata." };
        }

        if (order.CheckoutDetails.PaymentMethod != PaymentMethodStripeTest)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Pagamento Stripe non disponibile per questo ordine." };
        }

        var currentStatus = ToOrderStatus(order.Status);
        if (currentStatus == OrderStatus.PaymentFailed)
        {
            UpdateStripeReferences(order.CheckoutDetails, paymentIntentId, stripePaymentStatus, "failed");
            dbContext.SaveChanges();
            return new CheckoutPaymentResult
            {
                Success = true,
                OrderId = order.Id,
                FinalStatus = OrderStatus.PaymentFailed,
                PaymentStatus = order.CheckoutDetails.PaymentStatus
            };
        }

        if (currentStatus != OrderStatus.PaymentPending && currentStatus != OrderStatus.PaymentAuthorized)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Stato ordine non compatibile con fallimento Stripe." };
        }

        var now = GetCurrentTimestamp();
        if (!HasStockAlreadyRestored(order))
        {
            RestoreOrderStock(order);
        }

        AppendStatusHistory(order, currentStatus, OrderStatus.PaymentFailed, string.IsNullOrWhiteSpace(changedBy) ? "stripe" : changedBy, "Pagamento Stripe test fallito", stripeCheckoutSessionId, now);
        order.Status = (int)OrderStatus.PaymentFailed;
        order.UpdatedAt = now;
        UpdateStripeReferences(order.CheckoutDetails, paymentIntentId, stripePaymentStatus, "failed");
        order.CheckoutDetails.UpdatedAt = now;

        dbContext.SaveChanges();
        return new CheckoutPaymentResult
        {
            Success = true,
            OrderId = order.Id,
            FinalStatus = OrderStatus.PaymentFailed,
            PaymentStatus = order.CheckoutDetails.PaymentStatus
        };
    }

    public CheckoutPaymentResult ProcessTestPayment(string? customerEmail, int orderId, TestPaymentOutcome outcome)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        var order = dbContext.Orders
            .Include(existingOrder => existingOrder.Customer)
            .Include(existingOrder => existingOrder.Items)
            .ThenInclude(item => item.Product)
            .Include(existingOrder => existingOrder.StatusHistory)
            .Include(existingOrder => existingOrder.CheckoutDetails)
            .FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order is null || order.CheckoutDetails is null || order.Customer.Email.ToLower() != normalizedEmail)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Ordine non trovato." };
        }

        var currentStatus = ToOrderStatus(order.Status);
        if (currentStatus != OrderStatus.PaymentPending || order.CheckoutDetails.PaymentMethod != PaymentMethodTestCard)
        {
            return new CheckoutPaymentResult { ErrorMessage = "Pagamento test non disponibile per questo ordine." };
        }

        var now = GetCurrentTimestamp();
        var finalStatus = outcome switch
        {
            TestPaymentOutcome.Authorized => OrderStatus.Confirmed,
            TestPaymentOutcome.Failed => OrderStatus.PaymentFailed,
            _ => OrderStatus.PaymentPending
        };

        if (outcome == TestPaymentOutcome.Authorized)
        {
            var transactionReference = CreateTestTransactionReference(orderId);
            AppendStatusHistory(order, currentStatus, OrderStatus.PaymentAuthorized, normalizedEmail, "Pagamento test autorizzato", transactionReference, now);
            order.Status = (int)OrderStatus.PaymentAuthorized;
            AppendStatusHistory(order, OrderStatus.PaymentAuthorized, OrderStatus.Confirmed, normalizedEmail, "Ordine confermato dopo pagamento test", null, now);
            order.Status = (int)OrderStatus.Confirmed;
            order.CheckoutDetails.PaymentStatus = "authorized";
            order.CheckoutDetails.TestTransactionReference = transactionReference;
        }
        else if (outcome == TestPaymentOutcome.Failed)
        {
            RestoreOrderStock(order);
            AppendStatusHistory(order, currentStatus, OrderStatus.PaymentFailed, normalizedEmail, "Pagamento test fallito", null, now);
            order.Status = (int)OrderStatus.PaymentFailed;
            order.CheckoutDetails.PaymentStatus = "failed";
        }
        else
        {
            order.CheckoutDetails.PaymentStatus = "pending";
        }

        order.UpdatedAt = now;
        order.CheckoutDetails.UpdatedAt = now;
        var session = GetActiveCheckoutSession(normalizedEmail, trackChanges: true);
        if (session is not null)
        {
            session.CreatedOrderId = orderId;
            session.CurrentStep = (int)CheckoutStep.Result;
            TouchCheckoutSession(session, now);
        }

        dbContext.SaveChanges();
        return new CheckoutPaymentResult
        {
            Success = true,
            OrderId = orderId,
            FinalStatus = finalStatus
        };
    }

    public OrderDetailsViewModel? GetOrderDetails(int orderId, string? requesterEmail, bool isAdmin)
    {
        var normalizedEmail = NormalizeEmail(requesterEmail);
        var order = dbContext.Orders
            .AsNoTracking()
            .Include(existingOrder => existingOrder.Customer)
            .Include(existingOrder => existingOrder.Items)
            .ThenInclude(item => item.Product)
            .Include(existingOrder => existingOrder.StatusHistory)
            .Include(existingOrder => existingOrder.CheckoutDetails)
            .AsSplitQuery()
            .FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order is null || (!isAdmin && order.Customer.Email.ToLower() != normalizedEmail))
        {
            return null;
        }

        return new OrderDetailsViewModel
        {
            Order = MapOrder(order),
            CheckoutDetails = order.CheckoutDetails is null ? null : MapOrderCheckoutDetails(order.CheckoutDetails)
        };
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
        var order = CreateOrderEntity(customerEmail, items, OrderStatus.Pending);
        if (order is null)
        {
            return false;
        }

        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        return true;
    }

    public bool ChangeOrderStatus(int orderId, OrderStatus newStatus, string? changedBy = null, string? reason = null, string? correlationId = null)
    {
        var order = dbContext.Orders
            .Include(existingOrder => existingOrder.Items)
            .ThenInclude(item => item.Product)
            .Include(existingOrder => existingOrder.StatusHistory)
            .FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order is null)
        {
            return false;
        }

        var currentStatus = ToOrderStatus(order.Status);
        if (!OrderStatusTransitionPolicy.CanTransition(currentStatus, newStatus))
        {
            return false;
        }

        if (OrderStatusTransitionPolicy.RequiresReason(newStatus) && string.IsNullOrWhiteSpace(reason))
        {
            return false;
        }

        if (ShouldRestoreStock(currentStatus, newStatus) && HasStockAlreadyRestored(order))
        {
            return false;
        }

        if (ShouldRestoreStock(currentStatus, newStatus))
        {
            RestoreOrderStock(order);
        }

        var changedAt = GetCurrentTimestamp();
        order.Status = (int)newStatus;
        order.UpdatedAt = changedAt;
        AppendStatusHistory(order, currentStatus, newStatus, changedBy, reason, correlationId, changedAt);
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

    private CartEntity? GetActiveCart(string? customerEmail, bool trackChanges)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return null;
        }

        var query = dbContext.Carts
            .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
            .Where(cart => cart.CustomerEmail == normalizedEmail);

        var cart = trackChanges
            ? query.FirstOrDefault()
            : query.AsNoTracking().FirstOrDefault();

        if (cart is null)
        {
            return null;
        }

        if (cart.ExpiresAt > GetCurrentTimestamp())
        {
            return cart;
        }

        DeleteExpiredCart(normalizedEmail);
        return null;
    }

    private CartEntity GetOrCreateActiveCart(string normalizedEmail, DateTime now)
    {
        var cart = GetActiveCart(normalizedEmail, trackChanges: true);
        if (cart is not null)
        {
            return cart;
        }

        cart = new CartEntity
        {
            CustomerEmail = normalizedEmail,
            CreatedAt = now,
            UpdatedAt = now,
            ExpiresAt = CreateCartExpiration(now)
        };
        dbContext.Carts.Add(cart);
        return cart;
    }

    private void DeleteExpiredCart(string normalizedEmail)
    {
        var expiredCart = dbContext.Carts
            .Include(cart => cart.Items)
            .FirstOrDefault(cart => cart.CustomerEmail == normalizedEmail);

        if (expiredCart is null || expiredCart.ExpiresAt > GetCurrentTimestamp())
        {
            return;
        }

        dbContext.Carts.Remove(expiredCart);
        dbContext.SaveChanges();
    }

    private static CartViewModel MapCart(CartEntity entity)
    {
        return new CartViewModel
        {
            UpdatedAt = entity.UpdatedAt,
            ExpiresAt = entity.ExpiresAt,
            Items = entity.Items
                .OrderBy(item => item.Id)
                .Select(MapCartItem)
                .ToList()
        };
    }

    private static CartItemViewModel MapCartItem(CartItemEntity entity)
    {
        return new CartItemViewModel
        {
            ProductCode = entity.Product.Code,
            ProductName = entity.Product.Name,
            Description = entity.Product.Description ?? string.Empty,
            ImageUrl = entity.Product.ImageUrl ?? string.Empty,
            UnitPrice = entity.Product.Price,
            Quantity = entity.Quantity,
            Stock = entity.Product.StockQuantity
        };
    }

    private static void TouchCart(CartEntity cart, DateTime updatedAt)
    {
        cart.UpdatedAt = updatedAt;
        cart.ExpiresAt = CreateCartExpiration(updatedAt);
    }

    private static DateTime CreateCartExpiration(DateTime updatedAt)
    {
        return updatedAt.AddDays(CartRetentionDays);
    }

    private CheckoutSessionEntity? GetActiveCheckoutSession(string? customerEmail, bool trackChanges)
    {
        var normalizedEmail = NormalizeEmail(customerEmail);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return null;
        }

        var query = dbContext.CheckoutSessions.Where(session => session.CustomerEmail == normalizedEmail);
        var session = trackChanges
            ? query.FirstOrDefault()
            : query.AsNoTracking().FirstOrDefault();

        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt > GetCurrentTimestamp())
        {
            return session;
        }

        DeleteExpiredCheckoutSession(normalizedEmail);
        return null;
    }

    private void DeleteExpiredCheckoutSession(string normalizedEmail)
    {
        var expiredSession = dbContext.CheckoutSessions.FirstOrDefault(session => session.CustomerEmail == normalizedEmail);
        if (expiredSession is null || expiredSession.ExpiresAt > GetCurrentTimestamp())
        {
            return;
        }

        dbContext.CheckoutSessions.Remove(expiredSession);
        dbContext.SaveChanges();
    }

    private static void TouchCheckoutSession(CheckoutSessionEntity session, DateTime updatedAt)
    {
        session.UpdatedAt = updatedAt;
        session.ExpiresAt = updatedAt.AddHours(CheckoutRetentionHours);
    }

    private static CheckoutSessionViewModel MapCheckoutSession(CheckoutSessionEntity session, CartViewModel cart)
    {
        var model = new CheckoutSessionViewModel
        {
            CurrentStep = ToCheckoutStep(session.CurrentStep),
            Cart = cart,
            TotalItems = session.TotalItems,
            TotalAmount = session.TotalAmount,
            ShippingFullName = session.ShippingFullName ?? string.Empty,
            ShippingAddressLine = session.ShippingAddressLine ?? string.Empty,
            ShippingCity = session.ShippingCity ?? string.Empty,
            ShippingPostalCode = session.ShippingPostalCode ?? string.Empty,
            ShippingCountry = session.ShippingCountry ?? string.Empty,
            ShippingPhone = session.ShippingPhone ?? string.Empty,
            BillingSameAsShipping = session.BillingSameAsShipping,
            BillingFullName = session.BillingFullName ?? string.Empty,
            BillingAddressLine = session.BillingAddressLine ?? string.Empty,
            BillingCity = session.BillingCity ?? string.Empty,
            BillingPostalCode = session.BillingPostalCode ?? string.Empty,
            BillingCountry = session.BillingCountry ?? string.Empty,
            BillingVatNumber = session.BillingVatNumber ?? string.Empty,
            DeliveryMethod = session.DeliveryMethod,
            PaymentMethod = session.PaymentMethod,
            CreatedOrderId = session.CreatedOrderId,
            ExpiresAt = session.ExpiresAt
        };

        PopulateFormFields(model);
        return model;
    }

    private OrderEntity? CreateOrderEntity(string? customerEmail, IReadOnlyList<NewOrderItemViewModel> items, OrderStatus initialStatus)
    {
        if (string.IsNullOrWhiteSpace(customerEmail) || items.Count == 0)
        {
            return null;
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
            return null;
        }

        if (requestedItems.Select(item => item.ProductCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() != requestedItems.Count)
        {
            return null;
        }

        var requestedCodes = requestedItems.Select(item => item.ProductCode).ToList();
        var products = dbContext.Products
            .Where(product => requestedCodes.Contains(product.Code))
            .ToList();

        if (products.Count != requestedItems.Count)
        {
            return null;
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products.Single(product => product.Code == requestedItem.ProductCode);
            if (product.StockQuantity <= 0 || product.StockQuantity < requestedItem.Quantity)
            {
                return null;
            }
        }

        var normalizedEmail = NormalizeEmail(customerEmail);
        var customer = FindOrCreateCustomer(normalizedEmail);
        var createdAt = GetCurrentTimestamp();
        var order = new OrderEntity
        {
            OrderNumber = CreateNextOrderNumber(),
            CustomerId = customer.Id,
            TotalAmount = requestedItems.Sum(item =>
            {
                var product = products.Single(product => product.Code == item.ProductCode);
                return product.Price * item.Quantity;
            }),
            Status = (int)initialStatus,
            CreatedAt = createdAt
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

        AppendInitialStatusHistory(order, initialStatus, normalizedEmail, createdAt);
        return order;
    }

    private static OrderCheckoutDetailsEntity CreateOrderCheckoutDetails(int orderId, CheckoutSessionEntity session, OrderStatus initialStatus)
    {
        return new OrderCheckoutDetailsEntity
        {
            OrderId = orderId,
            ShippingFullName = session.ShippingFullName ?? string.Empty,
            ShippingAddressLine = session.ShippingAddressLine ?? string.Empty,
            ShippingCity = session.ShippingCity ?? string.Empty,
            ShippingPostalCode = session.ShippingPostalCode ?? string.Empty,
            ShippingCountry = session.ShippingCountry ?? string.Empty,
            ShippingPhone = session.ShippingPhone ?? string.Empty,
            BillingSameAsShipping = session.BillingSameAsShipping,
            BillingFullName = session.BillingFullName ?? string.Empty,
            BillingAddressLine = session.BillingAddressLine ?? string.Empty,
            BillingCity = session.BillingCity ?? string.Empty,
            BillingPostalCode = session.BillingPostalCode ?? string.Empty,
            BillingCountry = session.BillingCountry ?? string.Empty,
            BillingVatNumber = session.BillingVatNumber,
            DeliveryMethod = session.DeliveryMethod,
            PaymentMethod = session.PaymentMethod,
            PaymentStatus = initialStatus == OrderStatus.PaymentPending ? "pending" : "not-required",
            CreatedAt = GetCurrentTimestamp()
        };
    }

    private static OrderCheckoutDetailsViewModel MapOrderCheckoutDetails(OrderCheckoutDetailsEntity entity)
    {
        return new OrderCheckoutDetailsViewModel
        {
            ShippingFullName = entity.ShippingFullName,
            ShippingAddressLine = entity.ShippingAddressLine,
            ShippingCity = entity.ShippingCity,
            ShippingPostalCode = entity.ShippingPostalCode,
            ShippingCountry = entity.ShippingCountry,
            ShippingPhone = entity.ShippingPhone,
            BillingSameAsShipping = entity.BillingSameAsShipping,
            BillingFullName = entity.BillingFullName,
            BillingAddressLine = entity.BillingAddressLine,
            BillingCity = entity.BillingCity,
            BillingPostalCode = entity.BillingPostalCode,
            BillingCountry = entity.BillingCountry,
            BillingVatNumber = entity.BillingVatNumber ?? string.Empty,
            DeliveryMethod = entity.DeliveryMethod,
            PaymentMethod = entity.PaymentMethod,
            PaymentStatus = entity.PaymentStatus,
            TestTransactionReference = entity.TestTransactionReference ?? string.Empty,
            StripeCheckoutSessionId = entity.StripeCheckoutSessionId ?? string.Empty,
            StripePaymentIntentId = entity.StripePaymentIntentId ?? string.Empty,
            StripePaymentStatus = entity.StripePaymentStatus ?? string.Empty
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
            .Include(order => order.StatusHistory)
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
                .ToList(),
            StatusHistory = entity.StatusHistory
                .OrderByDescending(history => history.ChangedAt)
                .ThenByDescending(history => history.Id)
                .Select(MapOrderStatusHistory)
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

    private static OrderStatusHistory MapOrderStatusHistory(OrderStatusHistoryEntity entity)
    {
        return new OrderStatusHistory
        {
            FromStatus = entity.FromStatus.HasValue ? ToOrderStatus(entity.FromStatus.Value) : null,
            ToStatus = ToOrderStatus(entity.ToStatus),
            ChangedAt = entity.ChangedAt,
            ChangedBy = entity.ChangedBy ?? string.Empty,
            Reason = entity.Reason ?? string.Empty
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
            Stock = entity.StockQuantity,
            CarouselImages = entity.CarouselImages
                .OrderBy(image => image.DisplayOrder)
                .ThenBy(image => image.Id)
                .Select(MapProductCarouselImage)
                .ToList()
        };
    }

    private static ProductCarouselImage MapProductCarouselImage(ProductCarouselImageEntity entity)
    {
        return new ProductCarouselImage
        {
            ImageUrl = entity.ImageUrl,
            AltText = entity.AltText,
            DisplayOrder = entity.DisplayOrder
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

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static DateTime GetCurrentTimestamp()
    {
        return DateTime.UtcNow;
    }

    private static string? NormalizeImageUrl(string? imageUrl)
    {
        var normalizedImageUrl = (imageUrl ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalizedImageUrl) ? null : normalizedImageUrl;
    }

    private static string NormalizeText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = NormalizeText(value);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeCheckoutChoice(string? value, string defaultValue)
    {
        var normalized = NormalizeText(value).ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? defaultValue : normalized;
    }

    private static string ResolveShippingFullName(CheckoutAddressesViewModel model)
    {
        var structured = ComposeFullName(model.ShippingLastName, model.ShippingFirstName);
        return string.IsNullOrWhiteSpace(structured) ? NormalizeText(model.ShippingFullName) : structured;
    }

    private static string ResolveBillingFullName(CheckoutAddressesViewModel model)
    {
        var structured = ComposeFullName(model.BillingLastName, model.BillingFirstName);
        return string.IsNullOrWhiteSpace(structured) ? NormalizeText(model.BillingFullName) : structured;
    }

    private static string ResolveShippingAddressLine(CheckoutAddressesViewModel model)
    {
        var structured = ComposeAddressLine(model.ShippingStreet, model.ShippingStreetNumber);
        return string.IsNullOrWhiteSpace(structured) ? NormalizeText(model.ShippingAddressLine) : structured;
    }

    private static string ResolveBillingAddressLine(CheckoutAddressesViewModel model)
    {
        var structured = ComposeAddressLine(model.BillingStreet, model.BillingStreetNumber);
        return string.IsNullOrWhiteSpace(structured) ? NormalizeText(model.BillingAddressLine) : structured;
    }

    private static string ResolveShippingPhone(CheckoutAddressesViewModel model)
    {
        var structured = ComposePhone(model.ShippingPhonePrefix, model.ShippingPhoneNumber);
        return string.IsNullOrWhiteSpace(structured) ? NormalizeText(model.ShippingPhone) : structured;
    }

    private static string ResolveShippingCountry(CheckoutAddressesViewModel model)
    {
        return HasStructuredShippingFields(model) ? "Italia" : NormalizeText(model.ShippingCountry);
    }

    private static string ResolveBillingCountry(CheckoutAddressesViewModel model)
    {
        return HasStructuredBillingFields(model) ? "Italia" : NormalizeText(model.BillingCountry);
    }

    private static string ComposeFullName(string? lastName, string? firstName)
    {
        return JoinNonEmpty(NormalizeText(lastName), NormalizeText(firstName));
    }

    private static string ComposeAddressLine(string? street, string? streetNumber)
    {
        return JoinNonEmpty(NormalizeText(street), NormalizeText(streetNumber));
    }

    private static string ComposePhone(string? phonePrefix, string? phoneNumber)
    {
        return JoinNonEmpty(NormalizeText(phonePrefix), NormalizeText(phoneNumber));
    }

    private static string JoinNonEmpty(params string[] values)
    {
        return string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static void PopulateFormFields(CheckoutSessionViewModel model)
    {
        (model.ShippingLastName, model.ShippingFirstName) = SplitFirstToken(model.ShippingFullName);
        (model.ShippingStreet, model.ShippingStreetNumber) = SplitLastToken(model.ShippingAddressLine);
        (model.ShippingPhonePrefix, model.ShippingPhoneNumber) = SplitPhone(model.ShippingPhone);

        (model.BillingLastName, model.BillingFirstName) = SplitFirstToken(model.BillingFullName);
        (model.BillingStreet, model.BillingStreetNumber) = SplitLastToken(model.BillingAddressLine);
    }

    private static (string First, string Remainder) SplitFirstToken(string? value)
    {
        var normalized = NormalizeText(value);
        var separatorIndex = normalized.IndexOf(' ', StringComparison.Ordinal);
        return separatorIndex < 0
            ? (normalized, string.Empty)
            : (normalized[..separatorIndex], normalized[(separatorIndex + 1)..].Trim());
    }

    private static (string Remainder, string Last) SplitLastToken(string? value)
    {
        var normalized = NormalizeText(value);
        var separatorIndex = normalized.LastIndexOf(' ');
        return separatorIndex < 0
            ? (normalized, string.Empty)
            : (normalized[..separatorIndex].Trim(), normalized[(separatorIndex + 1)..]);
    }

    private static (string Prefix, string Number) SplitPhone(string? value)
    {
        var normalized = NormalizeText(value);
        if (!normalized.StartsWith("+", StringComparison.Ordinal))
        {
            return (string.Empty, normalized);
        }

        return SplitFirstToken(normalized);
    }

    private static bool AreShippingFieldsValid(CheckoutAddressesViewModel model)
    {
        return HasLegacyShippingFields(model) || HasStructuredShippingFields(model);
    }

    private static bool HasLegacyShippingFields(CheckoutAddressesViewModel model)
    {
        return !string.IsNullOrWhiteSpace(model.ShippingFullName)
            && !string.IsNullOrWhiteSpace(model.ShippingAddressLine)
            && !string.IsNullOrWhiteSpace(model.ShippingCity)
            && !string.IsNullOrWhiteSpace(model.ShippingPostalCode)
            && !string.IsNullOrWhiteSpace(model.ShippingCountry)
            && !string.IsNullOrWhiteSpace(model.ShippingPhone);
    }

    private static bool HasStructuredShippingFields(CheckoutAddressesViewModel model)
    {
        return !string.IsNullOrWhiteSpace(model.ShippingLastName)
            && !string.IsNullOrWhiteSpace(model.ShippingFirstName)
            && !string.IsNullOrWhiteSpace(model.ShippingPhonePrefix)
            && !string.IsNullOrWhiteSpace(model.ShippingPhoneNumber)
            && !string.IsNullOrWhiteSpace(model.ShippingStreet)
            && !string.IsNullOrWhiteSpace(model.ShippingStreetNumber)
            && !string.IsNullOrWhiteSpace(model.ShippingProvince)
            && !string.IsNullOrWhiteSpace(model.ShippingCity)
            && !string.IsNullOrWhiteSpace(model.ShippingPostalCode);
    }

    private static bool AreBillingFieldsValid(CheckoutAddressesViewModel model)
    {
        return model.BillingSameAsShipping
            || HasLegacyBillingFields(model)
            || HasStructuredBillingFields(model);
    }

    private static bool HasLegacyBillingFields(CheckoutAddressesViewModel model)
    {
        return !string.IsNullOrWhiteSpace(model.BillingFullName)
                && !string.IsNullOrWhiteSpace(model.BillingAddressLine)
                && !string.IsNullOrWhiteSpace(model.BillingCity)
                && !string.IsNullOrWhiteSpace(model.BillingPostalCode)
                && !string.IsNullOrWhiteSpace(model.BillingCountry);
    }

    private static bool HasStructuredBillingFields(CheckoutAddressesViewModel model)
    {
        return !string.IsNullOrWhiteSpace(model.BillingLastName)
            && !string.IsNullOrWhiteSpace(model.BillingFirstName)
            && !string.IsNullOrWhiteSpace(model.BillingStreet)
            && !string.IsNullOrWhiteSpace(model.BillingStreetNumber)
            && !string.IsNullOrWhiteSpace(model.BillingProvince)
            && !string.IsNullOrWhiteSpace(model.BillingCity)
            && !string.IsNullOrWhiteSpace(model.BillingPostalCode);
    }

    private static bool HasCheckoutAddresses(CheckoutSessionEntity session)
    {
        return !string.IsNullOrWhiteSpace(session.ShippingFullName)
            && !string.IsNullOrWhiteSpace(session.ShippingAddressLine)
            && !string.IsNullOrWhiteSpace(session.ShippingCity)
            && !string.IsNullOrWhiteSpace(session.ShippingPostalCode)
            && !string.IsNullOrWhiteSpace(session.ShippingCountry)
            && !string.IsNullOrWhiteSpace(session.ShippingPhone)
            && !string.IsNullOrWhiteSpace(session.BillingFullName)
            && !string.IsNullOrWhiteSpace(session.BillingAddressLine)
            && !string.IsNullOrWhiteSpace(session.BillingCity)
            && !string.IsNullOrWhiteSpace(session.BillingPostalCode)
            && !string.IsNullOrWhiteSpace(session.BillingCountry);
    }

    private static CheckoutStep ToCheckoutStep(int value)
    {
        return Enum.IsDefined(typeof(CheckoutStep), value)
            ? (CheckoutStep)value
            : CheckoutStep.Summary;
    }

    private static string CreateTestTransactionReference(int orderId)
    {
        return $"TEST-{orderId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private OrderEntity? GetStripeOrderForUpdate(string stripeCheckoutSessionId)
    {
        var normalizedSessionId = NormalizeOptionalText(stripeCheckoutSessionId);
        if (string.IsNullOrWhiteSpace(normalizedSessionId))
        {
            return null;
        }

        return dbContext.Orders
            .Include(existingOrder => existingOrder.Customer)
            .Include(existingOrder => existingOrder.Items)
            .ThenInclude(item => item.Product)
            .Include(existingOrder => existingOrder.StatusHistory)
            .Include(existingOrder => existingOrder.CheckoutDetails)
            .FirstOrDefault(existingOrder =>
                existingOrder.CheckoutDetails != null &&
                existingOrder.CheckoutDetails.StripeCheckoutSessionId == normalizedSessionId);
    }

    private static void UpdateStripeReferences(
        OrderCheckoutDetailsEntity details,
        string? paymentIntentId,
        string? stripePaymentStatus,
        string paymentStatus)
    {
        details.PaymentStatus = paymentStatus;
        details.StripePaymentIntentId = NormalizeOptionalText(paymentIntentId) ?? details.StripePaymentIntentId;
        details.StripePaymentStatus = NormalizeOptionalText(stripePaymentStatus) ?? details.StripePaymentStatus;
    }

    private static void AppendInitialStatusHistory(OrderEntity order, OrderStatus initialStatus, string changedBy, DateTime changedAt)
    {
        order.StatusHistory.Add(new OrderStatusHistoryEntity
        {
            FromStatus = null,
            ToStatus = (int)initialStatus,
            ChangedAt = changedAt,
            ChangedBy = NormalizeChangedBy(changedBy),
            Reason = "Order created"
        });
    }

    private static void AppendStatusHistory(
        OrderEntity order,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        string? changedBy,
        string? reason,
        string? correlationId,
        DateTime changedAt)
    {
        order.StatusHistory.Add(new OrderStatusHistoryEntity
        {
            FromStatus = (int)fromStatus,
            ToStatus = (int)toStatus,
            ChangedAt = changedAt,
            ChangedBy = NormalizeChangedBy(changedBy),
            Reason = NormalizeReason(reason),
            CorrelationId = NormalizeCorrelationId(correlationId)
        });
    }

    private static string? NormalizeChangedBy(string? changedBy)
    {
        var normalized = (changedBy ?? string.Empty).Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeReason(string? reason)
    {
        var normalized = (reason ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeCorrelationId(string? correlationId)
    {
        var normalized = (correlationId ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool ShouldRestoreStock(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (newStatus is OrderStatus.Cancelled or OrderStatus.PaymentFailed)
            && (currentStatus is OrderStatus.Pending
                or OrderStatus.PaymentPending
                or OrderStatus.PaymentAuthorized
                or OrderStatus.Confirmed);
    }

    private static bool HasStockAlreadyRestored(OrderEntity order)
    {
        return order.StatusHistory.Any(history => history.ToStatus is (int)OrderStatus.Cancelled or (int)OrderStatus.PaymentFailed);
    }

    private static void RestoreOrderStock(OrderEntity order)
    {
        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
        }
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
