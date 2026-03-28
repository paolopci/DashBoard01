using DashboardOrders.Models;

namespace DashboardOrders.Services;

public static class MockDataService
{
    private static readonly (string Name, decimal Price)[] OrderProductSeeds =
    {
        ("Laptop Pro 15", 1200m),
        ("Tastiera Meccanica", 85m),
        ("Monitor 4K 27 pollici", 450m),
        ("Mouse Wireless", 45m),
        ("Workstation Ultra", 2500m),
        ("Webcam HD Pro", 95m),
        ("SSD 1TB", 120m),
        ("Stampante Laser", 180m),
        ("Hub USB-C 7 porte", 50m),
        ("Cuffie Noise Cancel.", 250m)
    };

    private static readonly List<Category> Categories = GenerateCategories();
    private static readonly List<Product> Products = GenerateProducts(Categories);
    private static readonly List<Customer> Customers = GenerateCustomers(100);
    private static readonly List<Order> Orders = GenerateOrders(Customers);

    private static List<Category> GenerateCategories()
    {
        return new List<Category>
        {
            new() { Code = "CAT-001", Name = "Informatica", Description = "Dispositivi e strumenti per postazioni di lavoro professionali." },
            new() { Code = "CAT-002", Name = "Periferiche", Description = "Accessori e periferiche per input, stampa e produttivita quotidiana." },
            new() { Code = "CAT-003", Name = "Archiviazione", Description = "Soluzioni per memorizzazione, backup e gestione dei dati." },
            new() { Code = "CAT-004", Name = "Audio Video", Description = "Prodotti per videoconferenza, ascolto e contenuti multimediali." },
            new() { Code = "CAT-005", Name = "Networking", Description = "Apparati e accessori per connettivita e infrastruttura di rete." },
            new() { Code = "CAT-006", Name = "Ufficio", Description = "Strumenti tecnologici destinati all'operativita di ufficio." },
            new() { Code = "CAT-007", Name = "Gaming", Description = "Prodotti ad alte prestazioni per esperienze interattive avanzate." }
        };
    }

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

    private static List<Product> GenerateProducts(List<Category> categories)
    {
        var random = new Random(42);
        var products = new List<Product>();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var seededProducts = OrderProductSeeds
            .Select((seed, index) => new Product
            {
                Code = $"PRD-2026-{index + 1:D3}",
                Name = seed.Name,
                Category = categories[index % categories.Count],
                Description = $"Soluzione {categories[index % categories.Count].Name.ToLowerInvariant()} pensata per {seed.Name.ToLowerInvariant()}.",
                UnitCost = seed.Price,
                Stock = random.Next(12, 180)
            })
            .ToList();

        products.AddRange(seededProducts);

        foreach (var product in seededProducts)
        {
            usedNames.Add(product.Name);
        }

        var categoryTemplates = new Dictionary<string, string[]>
        {
            ["Informatica"] = new[] { "Notebook", "Desktop", "Mini PC", "Server", "Terminale", "Docking Station" },
            ["Periferiche"] = new[] { "Mouse", "Tastiera", "Scanner", "Lettore Barcode", "Stampante", "Trackpad" },
            ["Archiviazione"] = new[] { "SSD", "NAS", "Hard Disk", "Chiavetta USB", "Storage Array", "Backup Station" },
            ["Audio Video"] = new[] { "Webcam", "Cuffie", "Microfono", "Speaker", "Videobar", "Monitor" },
            ["Networking"] = new[] { "Router", "Switch", "Access Point", "Firewall", "Modulo SFP", "Bridge" },
            ["Ufficio"] = new[] { "Etichettatrice", "Distruggidocumenti", "Calcolatrice", "Proiettore", "Plotter", "Multifunzione" },
            ["Gaming"] = new[] { "Headset", "Controller", "Monitor", "Mouse Pad", "Console Desk", "Game Hub" }
        };
        var qualifiers = new[]
        {
            "Core", "Plus", "Edge", "Prime", "Flex", "Vision", "Elite", "Neo", "Smart", "Ultra",
            "Air", "Pro", "Max", "Compact", "Studio", "Office", "Link", "Pulse", "Sync", "Advance"
        };

        while (products.Count < 200)
        {
            var nextIndex = products.Count;
            var category = categories[nextIndex % categories.Count];
            var templatePool = categoryTemplates[category.Name];
            var template = templatePool[(nextIndex / categories.Count) % templatePool.Length];
            var qualifier = qualifiers[(nextIndex / (categories.Count * templatePool.Length)) % qualifiers.Length];
            var series = (nextIndex + 1).ToString("D3");
            var generatedName = $"{template} {qualifier} {series}";

            if (!usedNames.Add(generatedName))
            {
                continue;
            }

            var basePrice = 40m + ((nextIndex * 17) % 180) * 5m + (category.Name.Length * 3m);
            products.Add(new Product
            {
                Code = $"PRD-2026-{products.Count + 1:D3}",
                Name = generatedName,
                Category = category,
                Description = $"Articolo della categoria {category.Name.ToLowerInvariant()} progettato per ambienti operativi moderni.",
                UnitCost = decimal.Round(basePrice, 2),
                Stock = 10 + ((nextIndex * 13) % 240)
            });
        }

        return products;
    }

    private static List<Order> GenerateOrders(List<Customer> customers)
    {
        var states = Enum.GetValues<OrderStatus>();
        var random = new Random(42);
        var orders = new List<Order>();
        int orderIdCounter = 1;
        var orderCatalog = Products
            .Where(product => OrderProductSeeds.Any(seed => seed.Name == product.Name && seed.Price == product.UnitCost))
            .ToList();

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
                var items = GenerateOrderItems(orderCatalog, random);

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

    private static List<OrderItem> GenerateOrderItems(List<Product> products, Random random)
    {
        var itemsCount = random.Next(1, Math.Min(8, products.Count + 1));
        var selectedIndexes = Enumerable
            .Range(0, products.Count)
            .OrderBy(_ => random.Next())
            .Take(itemsCount);

        return selectedIndexes
            .Select(index =>
            {
                var product = products[index];

                return new OrderItem
                {
                    ProductName = product.Name,
                    Quantity = random.Next(1, 4),
                    UnitPrice = product.UnitCost
                };
            })
            .ToList();
    }

    public static List<Order> GetOrders() => Orders;
    public static List<Product> GetProducts() => Products;
    public static List<Category> GetCategories() => Categories;

    public static OrdersPageViewModel GetOrdersPageData(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc")
    {
        var normalizedSortBy = sortBy?.ToLowerInvariant() switch
        {
            "ordernumber" => "orderNumber",
            "date" => "date",
            "amount" => "amount",
            "status" => "status",
            _ => "date"
        };

        var normalizedSortDirection = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        var filteredOrders = customerId.HasValue
            ? Orders.Where(order => order.Customer.Id == customerId.Value).ToList()
            : Orders;
        var selectedCustomer = customerId.HasValue
            ? Customers.FirstOrDefault(customer => customer.Id == customerId.Value)
            : null;
        filteredOrders = (normalizedSortBy, normalizedSortDirection) switch
        {
            ("orderNumber", "asc") => filteredOrders
                .OrderBy(order => order.OrderNumber)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("orderNumber", "desc") => filteredOrders
                .OrderByDescending(order => order.OrderNumber)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("date", "asc") => filteredOrders
                .OrderBy(order => order.OrderDate)
                .ThenBy(order => order.OrderNumber)
                .ToList(),
            ("date", "desc") => filteredOrders
                .OrderByDescending(order => order.OrderDate)
                .ThenBy(order => order.OrderNumber)
                .ToList(),
            ("amount", "asc") => filteredOrders
                .OrderBy(order => order.TotalAmount)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("amount", "desc") => filteredOrders
                .OrderByDescending(order => order.TotalAmount)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("status", "asc") => filteredOrders
                .OrderBy(order => order.Status)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("status", "desc") => filteredOrders
                .OrderByDescending(order => order.Status)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            _ => filteredOrders
                .OrderByDescending(order => order.OrderDate)
                .ThenBy(order => order.OrderNumber)
                .ToList()
        };
        var normalizedPageSize = pageSize is 10 or 20 or 50 ? pageSize : 0;
        var totalPages = normalizedPageSize == 0
            ? 1
            : (int)Math.Ceiling(filteredOrders.Count / (double)normalizedPageSize);
        var normalizedPage = Math.Clamp(page, 1, Math.Max(totalPages, 1));
        var pagedOrders = normalizedPageSize == 0
            ? filteredOrders
            : filteredOrders
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

        return new OrdersPageViewModel
        {
            Orders = pagedOrders,
            TotalOrders = filteredOrders.Count,
            TotalRevenue = filteredOrders.Where(order => order.Status != OrderStatus.Cancelled).Sum(order => order.TotalAmount),
            PendingOrders = filteredOrders.Count(order => order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing),
            ShippedOrders = filteredOrders.Count(order => order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered),
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = normalizedPage,
            PageSize = normalizedPageSize,
            TotalPages = totalPages,
            SelectedCustomerId = selectedCustomer?.Id,
            SelectedCustomerName = selectedCustomer?.Name ?? string.Empty
        };
    }

    public static CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
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
        var normalizedSearch = (search ?? string.Empty).Trim();

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

        if (normalizedSearch.Length >= 3)
        {
            customerSummaries = customerSummaries
                .Where(summary =>
                    summary.Customer.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                    summary.Customer.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

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
            : Math.Max(1, (int)Math.Ceiling(customerSummaries.Count / (double)normalizedPageSize));
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
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = normalizedPage,
            PageSize = normalizedPageSize,
            TotalPages = totalPages
        };
    }

    public static ProductsPageViewModel GetProductsPageData(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "")
    {
        var normalizedSortBy = sortBy?.ToLowerInvariant() switch
        {
            "code" => "code",
            "name" => "name",
            "category" => "category",
            "description" => "description",
            "unitcost" => "unitCost",
            "stock" => "stock",
            _ => "name"
        };

        var normalizedSortDirection = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";

        var normalizedCategoryCode = (categoryCode ?? string.Empty).Trim();
        var filteredProducts = string.IsNullOrWhiteSpace(normalizedCategoryCode)
            ? Products
            : Products
                .Where(product => string.Equals(product.Category.Code, normalizedCategoryCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var sortedProducts = (normalizedSortBy, normalizedSortDirection) switch
        {
            ("code", "asc") => filteredProducts.OrderBy(product => product.Code).ToList(),
            ("code", "desc") => filteredProducts.OrderByDescending(product => product.Code).ToList(),
            ("name", "asc") => filteredProducts.OrderBy(product => product.Name).ThenBy(product => product.Code).ToList(),
            ("name", "desc") => filteredProducts.OrderByDescending(product => product.Name).ThenBy(product => product.Code).ToList(),
            ("category", "asc") => filteredProducts.OrderBy(product => product.Category.Name).ThenBy(product => product.Name).ToList(),
            ("category", "desc") => filteredProducts.OrderByDescending(product => product.Category.Name).ThenBy(product => product.Name).ToList(),
            ("description", "asc") => filteredProducts.OrderBy(product => product.Description).ThenBy(product => product.Name).ToList(),
            ("description", "desc") => filteredProducts.OrderByDescending(product => product.Description).ThenBy(product => product.Name).ToList(),
            ("unitCost", "asc") => filteredProducts.OrderBy(product => product.UnitCost).ThenBy(product => product.Name).ToList(),
            ("unitCost", "desc") => filteredProducts.OrderByDescending(product => product.UnitCost).ThenBy(product => product.Name).ToList(),
            ("stock", "asc") => filteredProducts.OrderBy(product => product.Stock).ThenBy(product => product.Name).ToList(),
            ("stock", "desc") => filteredProducts.OrderByDescending(product => product.Stock).ThenBy(product => product.Name).ToList(),
            _ => filteredProducts.OrderBy(product => product.Code).ToList()
        };

        var normalizedPageSize = pageSize is 10 or 20 or 50 ? pageSize : 0;
        var totalPages = normalizedPageSize == 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(sortedProducts.Count / (double)normalizedPageSize));
        var normalizedPage = Math.Clamp(page, 1, Math.Max(totalPages, 1));
        var pagedProducts = normalizedPageSize == 0
            ? sortedProducts
            : sortedProducts
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

        return new ProductsPageViewModel
        {
            Products = pagedProducts,
            Categories = Categories.OrderBy(category => category.Name).ToList(),
            TotalProducts = sortedProducts.Count,
            TotalCategories = Categories.Count,
            TotalStock = sortedProducts.Sum(product => product.Stock),
            InventoryValue = sortedProducts.Sum(product => product.UnitCost * product.Stock),
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            SelectedCategoryCode = normalizedCategoryCode,
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
