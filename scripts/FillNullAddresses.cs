using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DashboardOrders.Data;
using DashboardOrders.Data.Entities;

namespace DashboardOrders;

public static class FillNullAddressesProgram
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("secret.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DashboardAppDb");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Errore: Connection string non trovata in secret.json");
            return;
        }

        Console.WriteLine($"Connessione a: {connectionString}");

        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new DashboardOrdersDbContext(options);

        // Passo 1: Resetta TUTTI gli indirizzi a NULL
        var allCustomers = await context.Customers.ToListAsync();
        foreach (var customer in allCustomers)
        {
            customer.Address = null;
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"Reset: {allCustomers.Count} indirizzi resettati a NULL");

        // Passo 2: Genera nuovi indirizzi UNIVI per TUTTI i clienti
        var customers = await context.Customers.ToListAsync();
        Console.WriteLine($"Generazione indirizzi unici per {customers.Count} clienti...");

        // Database di indirizzi italiani (città, vie, piazze)
        var cities = new[]
        {
            "Roma", "Milano", "Torino", "Napoli", "Firenze", "Bologna",
            "Venezia", "Genova", "Palermo", "Bari", "Catania", "Verona",
            "Messina", "Padova", "Trieste", "Brescia", "Parma", "Prato",
            "Reggio Calabria", "Modena", "Reggio Emilia", "Perugia",
            "Livorno", "Ravenna", "Cagliari", "Foggia", "Rimini", "Salerno"
        };

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

                if (attempts > 10)
                {
                    Console.WriteLine($"Attenzione: Problema generando indirizzo per ID {customer.Id}");
                    break;
                }
            } while (usedAddresses.Contains(address));

            customer.Address = address;
            Console.WriteLine($"ID {customer.Id}: {address}");
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"\nOperazione completata: {customers.Count} indirizzi univoci generati con successo.");
    }
}
