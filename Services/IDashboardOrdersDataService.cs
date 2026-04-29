using DashboardOrders.Models;

namespace DashboardOrders.Services;

public interface IDashboardOrdersDataService
{
    DashboardViewModel GetDashboardData(int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "");
    OrdersPageViewModel GetOrdersPageData(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "");
    OrdersPageViewModel GetOrdersPageDataForCustomerEmail(string? email, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "");
    CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "");
    ProductsPageViewModel GetProductsPageData(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "");

    // Metodi paginati ritornano PaginationResult<T>
    PaginationResult<Order> GetOrders(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "");
    PaginationResult<CustomerOrdersSummaryViewModel> GetCustomers(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "");
    PaginationResult<Product> GetProducts(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "");

    Customer? GetCustomer(int id);
    Customer? GetCustomerByEmail(string? email);
    bool UpdateCustomer(Customer customer);
    Product? GetProduct(string code);
    List<Product> GetAvailableProducts();
    CartViewModel GetCart(string? customerEmail);
    int GetCartItemsCount(string? customerEmail);
    bool AddOrUpdateCartItem(string? customerEmail, string productCode, int quantity);
    bool UpdateCartItemQuantity(string? customerEmail, string productCode, int quantity);
    bool RemoveCartItem(string? customerEmail, string productCode);
    bool ClearCart(string? customerEmail);
    bool StartCheckout(string? customerEmail);
    CheckoutSessionViewModel? GetCheckout(string? customerEmail);
    List<string> GetItalianProvinces();
    List<string> GetItalianCities(string? provinceName);
    List<string> GetItalianPostalCodes(string? provinceName, string? cityName);
    bool SaveCheckoutAddresses(string? customerEmail, CheckoutAddressesViewModel model);
    bool SaveCheckoutOptions(string? customerEmail, CheckoutOptionsViewModel model);
    CheckoutConfirmResult ConfirmCheckout(string? customerEmail);
    CheckoutPaymentResult ProcessTestPayment(string? customerEmail, int orderId, TestPaymentOutcome outcome);
    bool SaveStripeCheckoutSession(string? customerEmail, int orderId, StripeCheckoutSessionResult session);
    int? GetOrderIdByStripeCheckoutSession(string stripeCheckoutSessionId);
    CheckoutPaymentResult CompleteStripePayment(string stripeCheckoutSessionId, string? paymentIntentId, string? stripePaymentStatus, string? changedBy = null);
    CheckoutPaymentResult FailStripePayment(string stripeCheckoutSessionId, string? paymentIntentId, string? stripePaymentStatus, string? changedBy = null);
    OrderDetailsViewModel? GetOrderDetails(int orderId, string? requesterEmail, bool isAdmin);
    bool CreateProduct(Product product);
    bool UpdateProduct(Product product);
    bool CreateOrder(string? customerEmail, string productCode, int quantity);
    bool CreateOrder(string? customerEmail, IReadOnlyList<NewOrderItemViewModel> items);
    bool ChangeOrderStatus(int orderId, OrderStatus newStatus, string? changedBy = null, string? reason = null, string? correlationId = null);
    CategoryPageViewModel GetCategoryPageData(string sortBy = "code", string sortDirection = "asc");
    Category? GetCategory(string code);
    bool CreateCategory(Category category);
    bool UpdateCategory(Category category);
    bool DeleteCategory(string code);
}
