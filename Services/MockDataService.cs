using DashboardOrders.Models;

namespace DashboardOrders.Services;

public static class MockDataService
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
                ImageUrl = CreateProductImageUrl(seed.Name, categories[index % categories.Count].Name, index + 1),
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
                ImageUrl = CreateProductImageUrl(generatedName, category.Name, products.Count + 1),
                UnitCost = decimal.Round(basePrice, 2),
                Stock = 10 + ((nextIndex * 13) % 240)
            });
        }

        return products;
    }

    private static string CreateProductImageUrl(string productName, string categoryName, int productNumber)
    {
        var keywords = GetProductImageKeywords(productName, categoryName);
        return $"https://loremflickr.com/320/240/{keywords}?lock={productNumber}";
    }

    private static string GetProductImageKeywords(string productName, string categoryName)
    {
        var name = productName.ToLowerInvariant();

        if (name.Contains("calcolatrice"))
        {
            return "calculator,office/all";
        }

        if (name.Contains("cuffie") || name.Contains("headset"))
        {
            return "headphones,audio/all";
        }

        if (name.Contains("laptop") || name.Contains("notebook"))
        {
            return "laptop,computer/all";
        }

        if (name.Contains("tastiera"))
        {
            return "keyboard,computer/all";
        }

        if (name.Contains("monitor"))
        {
            return "monitor,computer/all";
        }

        if (name.Contains("mouse"))
        {
            return "mouse,computer/all";
        }

        if (name.Contains("workstation") || name.Contains("desktop") || name.Contains("mini pc"))
        {
            return "desktop,computer/all";
        }

        if (name.Contains("webcam") || name.Contains("videobar"))
        {
            return "webcam,video/all";
        }

        if (name.Contains("ssd") || name.Contains("hard disk") || name.Contains("storage") || name.Contains("chiavetta"))
        {
            return "harddrive,storage/all";
        }

        if (name.Contains("stampante") || name.Contains("multifunzione") || name.Contains("plotter"))
        {
            return "printer,office/all";
        }

        if (name.Contains("hub") || name.Contains("usb"))
        {
            return "usb,technology/all";
        }

        if (name.Contains("scanner") || name.Contains("barcode"))
        {
            return "scanner,office/all";
        }

        if (name.Contains("nas") || name.Contains("backup") || name.Contains("server"))
        {
            return "server,storage/all";
        }

        if (name.Contains("microfono"))
        {
            return "microphone,audio/all";
        }

        if (name.Contains("speaker"))
        {
            return "speaker,audio/all";
        }

        if (name.Contains("router") || name.Contains("switch") || name.Contains("firewall") || name.Contains("sfp"))
        {
            return "network,router/all";
        }

        if (name.Contains("access point") || name.Contains("bridge"))
        {
            return "wifi,network/all";
        }

        if (name.Contains("etichettatrice"))
        {
            return "label,printer/all";
        }

        if (name.Contains("distruggidocumenti"))
        {
            return "shredder,office/all";
        }

        if (name.Contains("proiettore"))
        {
            return "projector,office/all";
        }

        if (name.Contains("controller"))
        {
            return "gamepad,gaming/all";
        }

        if (name.Contains("console") || name.Contains("game hub"))
        {
            return "console,gaming/all";
        }

        return categoryName switch
        {
            "Audio Video" => "audio,video/all",
            "Networking" => "network,technology/all",
            "Archiviazione" => "storage,technology/all",
            "Ufficio" => "office,technology/all",
            "Gaming" => "gaming,technology/all",
            _ => "technology,product/all"
        };
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
    public static List<Customer> GetCustomers() => Customers;

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

    private static List<Order> SortOrders(IEnumerable<Order> orders, string sortBy, string sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            ("orderNumber", "asc") => orders
                .OrderBy(order => order.OrderNumber)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("orderNumber", "desc") => orders
                .OrderByDescending(order => order.OrderNumber)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("date", "asc") => orders
                .OrderBy(order => order.OrderDate)
                .ThenBy(order => order.OrderNumber)
                .ToList(),
            ("date", "desc") => orders
                .OrderByDescending(order => order.OrderDate)
                .ThenBy(order => order.OrderNumber)
                .ToList(),
            ("amount", "asc") => orders
                .OrderBy(order => order.TotalAmount)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("amount", "desc") => orders
                .OrderByDescending(order => order.TotalAmount)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("status", "asc") => orders
                .OrderBy(order => order.Status)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            ("status", "desc") => orders
                .OrderByDescending(order => order.Status)
                .ThenByDescending(order => order.OrderDate)
                .ToList(),
            _ => orders
                .OrderByDescending(order => order.OrderDate)
                .ThenBy(order => order.OrderNumber)
                .ToList()
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

    public static OrdersPageViewModel GetOrdersPageData(int? customerId = null, int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "", string dateFrom = "", string dateTo = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, OrdersSortColumns, "date");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var normalizedDateFrom = NormalizeDateFilter(dateFrom);
        var normalizedDateTo = NormalizeDateFilter(dateTo);

        var filteredOrders = customerId.HasValue
            ? Orders.Where(order => order.Customer.Id == customerId.Value).ToList()
            : Orders;
        var selectedCustomer = customerId.HasValue
            ? Customers.FirstOrDefault(customer => customer.Id == customerId.Value)
            : null;
        filteredOrders = ApplySearch(filteredOrders, normalizedSearch, order =>
            order.OrderNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Items.Any(item => item.ProductName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)) ||
            OrderStatusPresentation.FromStatus(order.Status).Label.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        filteredOrders = ApplyOrderDateFilter(filteredOrders, normalizedDateFrom, normalizedDateTo);
        filteredOrders = SortOrders(filteredOrders, normalizedSortBy, normalizedSortDirection);
        var pagedOrders = ApplyPaging(filteredOrders, page, pageSize);

        return new OrdersPageViewModel
        {
            Orders = pagedOrders.Items,
            TotalOrders = filteredOrders.Count,
            TotalRevenue = filteredOrders.Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(order.Status)).Sum(order => order.TotalAmount),
            PendingOrders = filteredOrders.Count(order => OrderStatusMetricsPolicy.IsOperationallyActive(order.Status)),
            ShippedOrders = filteredOrders.Count(order => OrderStatusMetricsPolicy.IsFulfillmentCompleted(order.Status)),
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            CurrentPage = pagedOrders.CurrentPage,
            PageSize = pagedOrders.PageSize,
            TotalPages = pagedOrders.TotalPages,
            SelectedCustomerId = selectedCustomer?.Id,
            SelectedCustomerName = selectedCustomer?.Name ?? string.Empty
        };
    }

    public static CategoryPageViewModel GetCategoryPageData(string sortBy = "code", string sortDirection = "asc")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, CategorySortColumns, "code");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "asc");
        var orderedCategories = SortCategories(Categories, normalizedSortBy, normalizedSortDirection);

        return new CategoryPageViewModel
        {
            Categories = orderedCategories,
            TotalCategories = orderedCategories.Count,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection
        };
    }

    public static CustomersPageViewModel GetCustomersPageData(int page = 1, int pageSize = 10, string sortBy = "totalAmount", string sortDirection = "desc", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, CustomerSortColumns, "totalAmount");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);

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
                        .Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(order.Status))
                        .Sum(order => order.TotalAmount)
                };
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

    public static ProductsPageViewModel GetProductsPageData(int page = 1, int pageSize = 10, string sortBy = "code", string sortDirection = "asc", string categoryCode = "", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, ProductSortColumns, "name");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "asc");
        var normalizedSearch = NormalizeSearch(search);

        var normalizedCategoryCode = NormalizeSearch(categoryCode);
        var filteredProducts = string.IsNullOrWhiteSpace(normalizedCategoryCode)
            ? Products
            : Products
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
            Categories = Categories.OrderBy(category => category.Name).ToList(),
            TotalProducts = sortedProducts.Count,
            TotalCategories = Categories.Count,
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

    public static DashboardViewModel GetDashboardData(int page = 1, int pageSize = 10, string sortBy = "date", string sortDirection = "desc", string search = "")
    {
        var normalizedSortBy = NormalizeSortBy(sortBy, DashboardSortColumns, "date");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection, "desc");
        var normalizedSearch = NormalizeSearch(search);
        var filteredOrders = ApplySearch(Orders, normalizedSearch, order =>
            order.OrderNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Customer.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            order.Product.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            OrderStatusPresentation.FromStatus(order.Status).Label.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        var sortedOrders = SortDashboardOrders(filteredOrders, normalizedSortBy, normalizedSortDirection);

        var pagedOrders = ApplyPaging(sortedOrders, page, pageSize);

        return new DashboardViewModel
        {
            RecentOrders    = pagedOrders.Items,
            TotalOrders     = sortedOrders.Count,
            TotalRevenue    = sortedOrders.Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant(order.Status)).Sum(order => order.TotalAmount),
            PendingOrders   = sortedOrders.Count(order => OrderStatusMetricsPolicy.IsOperationallyActive(order.Status)),
            DeliveredOrders = sortedOrders.Count(o => o.Status == OrderStatus.Delivered),
            ActiveCustomers = Customers.Count,
            SearchTerm = normalizedSearch,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CurrentPage = pagedOrders.CurrentPage,
            PageSize = pagedOrders.PageSize,
            TotalPages = pagedOrders.TotalPages
        };
    }
}
