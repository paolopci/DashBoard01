using DashboardOrders.Data;
using DashboardOrders.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secret.json", optional: true, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DashboardAppDb")
    ?? throw new InvalidOperationException("Connection string 'DashboardAppDb' non configurata. Crea secret.json con la connection string locale.");
var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
{
    TrustServerCertificate = true
};
builder.Services.AddDbContext<DashboardOrdersDbContext>(options =>
    options.UseSqlServer(
        sqlConnectionStringBuilder.ConnectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AddScoped<IDashboardOrdersDataService, DashboardOrdersDataService>();
builder.Services.AddScoped<DashboardOrdersDatabaseSeeder>();

var app = builder.Build();
if (args.Any(arg => string.Equals(arg, "--seed-database", StringComparison.OrdinalIgnoreCase)))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DashboardOrdersDatabaseSeeder>();
    await seeder.SeedAsync();
    return;
}

if (args.Any(arg => string.Equals(arg, "--generate-unique-addresses", StringComparison.OrdinalIgnoreCase)))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DashboardOrdersDbContext>();

    // Cities database
    var cities = new[]
    {
        "Roma", "Milano", "Torino", "Napoli", "Firenze", "Bologna",
        "Venezia", "Genova", "Palermo", "Bari", "Catania", "Verona",
        "Messina", "Padova", "Trieste", "Brescia", "Parma", "Prato",
        "Modena", "Reggio Emilia", "Perugia", "Livorno", "Ravenna",
        "Cagliari", "Foggia", "Rimini", "Salerno"
    };

    // Street types with numbers
    var streetTypes = new[]
    {
        "Via Roma", "Viale Roma", "Piazza Roma",
        "Via Milano", "Viale Milano", "Piazza Milano",
        "Via Torino", "Viale Torino", "Piazza Torino",
        "Via Firenze", "Viale Firenze", "Piazza Firenze",
        "Via Bologna", "Viale Bologna", "Piazza Bologna",
        "Via Venezia", "Viale Venezia", "Piazza Venezia",
        "Via Genova", "Viale Genova", "Piazza Genova",
        "Via Napoli", "Viale Napoli", "Piazza Napoli",
        "Via Dante", "Viale Dante", "Piazza Dante",
        "Via Manzoni", "Viale Manzoni", "Piazza Manzoni",
        "Via Verdi", "Viale Verdi", "Piazza Verdi",
        "Via Garibaldi", "Viale Garibaldi", "Piazza Garibaldi",
        "Via Mazzini", "Viale Mazzini", "Piazza Mazzini",
        "Via Colosseo", "Via del Corso", "Via Nazionale",
        "Piazza del Duomo", "Piazza della Repubblica", "Piazza Navona"
    };

    var rng = new Random();
    var usedAddresses = new HashSet<string>();

    var customers = await context.Customers.ToListAsync();
    Console.WriteLine($"Totale clienti: {customers.Count}");

    foreach (var customer in customers)
    {
        string address;
        int attempts = 0;
        do
        {
            var randomStreetType = streetTypes[rng.Next(streetTypes.Length)];
            var randomCity = cities[rng.Next(cities.Length)];
            var randomNumber = rng.Next(1, 500);
            address = $"{randomStreetType} {randomNumber}, {randomCity}";
            attempts++;
        } while (usedAddresses.Contains(address) && attempts < 20);

        usedAddresses.Add(address);
        customer.Address = address;
    }

    await context.SaveChangesAsync();
    Console.WriteLine($"Operazione completata: {customers.Count} indirizzi univoci generati e salvati.");
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
