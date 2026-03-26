using DashboardOrders.Models;

namespace DashboardOrders.Services;

public static class MockDataService
{
    private static readonly List<Customer> Customers = GenerateCustomers(100);
    private static readonly List<Order> Orders = GenerateOrders(Customers);

    private static List<Customer> GenerateCustomers(int count)
    {
        var firstNames = new[] { "Marco", "Giulia", "Luca", "Sara", "Antonio", "Elena", "Roberto", "Chiara", "Alessandro", "Sofia", "Paolo", "Francesca", "Matteo", "Alice", "Davide" };
        var lastNames = new[] { "Rossi", "Ferrari", "Bianchi", "Esposito", "Ricci", "Moretti", "Marini", "Lombardi", "Barbieri", "Galli", "Fontana", "Conti", "Bruno", "Rizzo", "Gallo" };
        var random = new Random(42);
        
        var list = new List<Customer>();
        for (int i = 1; i <= count; i++)
        {
            var fn = firstNames[random.Next(firstNames.Length)];
            var ln = lastNames[random.Next(lastNames.Length)];
            var name = $"{fn} {ln}";
            list.Add(new Customer
            {
                Id = i,
                Name = name,
                Email = $"{fn.ToLower()}.{ln.ToLower()}{i}@example.com",
                Phone = $"+39 0{random.Next(10, 99)} {random.Next(1000000, 9999999)}",
                AvatarInitials = $"{fn[0]}{ln[0]}"
            });
        }
        return list;
    }

    private static List<Order> GenerateOrders(List<Customer> customers)
    {
        var products = new[] {
            "Laptop Pro 15", "Tastiera Meccanica", "Monitor 4K 27 pollici", "Mouse Wireless",
            "Workstation Ultra", "Webcam HD Pro", "SSD 1TB", "Stampante Laser", "Hub USB-C 7 porte", "Cuffie Noise Cancel." 
        };
        var prices = new[] { 1200m, 85m, 450m, 45m, 2500m, 95m, 120m, 180m, 50m, 250m };
        var states = Enum.GetValues<OrderStatus>();
        var random = new Random(42);
        var orders = new List<Order>();
        int orderIdCounter = 1;

        foreach (var customer in customers)
        {
            int ordersCount = customer.Id switch
            {
                <= 35 => 0,
                <= 60 => 1,
                _ => random.Next(2, 6)
            };

            for (int j = 0; j < ordersCount; j++)
            {
                var orderNumber = $"ORD-2026-{orderIdCounter:D3}";
                var items = GenerateOrderItems(products, prices, random);

                orders.Add(new Order
                {
                    Id = orderIdCounter,
                    OrderNumber = orderNumber,
                    Customer = customer,
                    OrderDate = DateTime.Now.AddDays(-random.Next(1, 60)),
                    Items = items,
                    TotalAmount = items.Sum(item => item.TotalPrice),
                    Status = states[random.Next(states.Length)]
                });

                orderIdCounter++;
            }
        }

        return orders.OrderByDescending(o => o.OrderDate).ToList();
    }

    private static List<OrderItem> GenerateOrderItems(string[] products, decimal[] prices, Random random)
    {
        var itemsCount = random.Next(1, 8);
        var selectedIndexes = Enumerable
            .Range(0, products.Length)
            .OrderBy(_ => random.Next())
            .Take(itemsCount);

        return selectedIndexes
            .Select(index => new OrderItem
            {
                ProductName = products[index],
                Quantity = random.Next(1, 4),
                UnitPrice = prices[index]
            })
            .ToList();
    }

    public static List<Order> GetOrders() => Orders;

    public static OrdersPageViewModel GetOrdersPageData(int? customerId = null)
    {
        var filteredOrders = customerId.HasValue
            ? Orders.Where(order => order.Customer.Id == customerId.Value).ToList()
            : Orders;
        var selectedCustomer = customerId.HasValue
            ? Customers.FirstOrDefault(customer => customer.Id == customerId.Value)
            : null;

        return new OrdersPageViewModel
        {
            Orders = filteredOrders,
            TotalOrders = filteredOrders.Count,
            TotalRevenue = filteredOrders.Where(order => order.Status != OrderStatus.Cancelled).Sum(order => order.TotalAmount),
            PendingOrders = filteredOrders.Count(order => order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing),
            ShippedOrders = filteredOrders.Count(order => order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered),
            SelectedCustomerId = selectedCustomer?.Id,
            SelectedCustomerName = selectedCustomer?.Name ?? string.Empty
        };
    }

    public static CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc")
    {
        var normalizedSortBy = sortBy?.ToLowerInvariant() switch
        {
            "customer" => "customer",
            "orderscount" => "ordersCount",
            "totalamount" => "totalAmount",
            _ => "totalAmount"
        };

        var normalizedSortDirection = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        var customerSummaries = Customers
            .Select(customer =>
            {
                var customerOrders = Orders
                    .Where(order => order.Customer.Id == customer.Id)
                    .ToList();

                return new CustomerOrdersSummaryViewModel
                {
                    Customer = customer,
                    OrdersCount = customerOrders.Count,
                    TotalOrdersAmount = customerOrders
                        .Where(order => order.Status != OrderStatus.Cancelled)
                        .Sum(order => order.TotalAmount)
                };
            })
            .ToList();

        customerSummaries = (normalizedSortBy, normalizedSortDirection) switch
        {
            ("customer", "asc") => customerSummaries
                .OrderBy(summary => summary.Customer.Name)
                .ThenByDescending(summary => summary.TotalOrdersAmount)
                .ToList(),
            ("customer", "desc") => customerSummaries
                .OrderByDescending(summary => summary.Customer.Name)
                .ThenByDescending(summary => summary.TotalOrdersAmount)
                .ToList(),
            ("ordersCount", "asc") => customerSummaries
                .OrderBy(summary => summary.OrdersCount)
                .ThenBy(summary => summary.Customer.Name)
                .ToList(),
            ("ordersCount", "desc") => customerSummaries
                .OrderByDescending(summary => summary.OrdersCount)
                .ThenBy(summary => summary.Customer.Name)
                .ToList(),
            ("totalAmount", "asc") => customerSummaries
                .OrderBy(summary => summary.TotalOrdersAmount)
                .ThenBy(summary => summary.Customer.Name)
                .ToList(),
            _ => customerSummaries
                .OrderByDescending(summary => summary.TotalOrdersAmount)
                .ThenBy(summary => summary.Customer.Name)
                .ToList()
        };

        var normalizedPageSize = pageSize is 10 or 20 or 50 ? pageSize : 0;
        var totalPages = normalizedPageSize == 0
            ? 1
            : (int)Math.Ceiling(customerSummaries.Count / (double)normalizedPageSize);
        var normalizedPage = Math.Clamp(page, 1, Math.Max(totalPages, 1));
        var pagedCustomers = normalizedPageSize == 0
            ? customerSummaries
            : customerSummaries
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

        return new CustomersPageViewModel
        {
            Customers = pagedCustomers,
            TotalCustomers = customerSummaries.Count,
            CustomersWithOrders = customerSummaries.Count(summary => summary.OrdersCount > 0),
            TotalRevenue = customerSummaries.Sum(summary => summary.TotalOrdersAmount),
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = normalizedPage,
            PageSize = normalizedPageSize,
            TotalPages = totalPages
        };
    }

    public static DashboardViewModel GetDashboardData()
    {
        return new DashboardViewModel
        {
            RecentOrders    = Orders.Take(10).ToList(),
            TotalOrders     = Orders.Count,
            TotalRevenue    = Orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            PendingOrders   = Orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing),
            DeliveredOrders = Orders.Count(o => o.Status == OrderStatus.Delivered),
            ActiveCustomers = Customers.Count
        };
    }
}
