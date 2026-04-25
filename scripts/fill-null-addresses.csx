#r "Microsoft.EntityFrameworkCore"
#r "Microsoft.EntityFrameworkCore.SqlServer"
#r "DashboardOrders/bin/Debug/net8.0/DashboardOrders.dll"

using Microsoft.EntityFrameworkCore;
using DashboardOrders.Data;
using DashboardOrders.Data.Entities;

var secretContent = File.ReadAllText("secret.json");
var connectionStart = secretContent.IndexOf("\"DashboardAppDb\"");
if (connectionStart == -1)
{
    Console.WriteLine("Errore: Connection string non trovata in secret.json");
    return;
}

var startIndex = secretContent.IndexOf(":", connectionStart);
var quotesStart = secretContent.IndexOf("\"", startIndex);
var connectionStringEnd = secretContent.IndexOf("\"", quotesStart + 1);
var connectionString = secretContent.Substring(quotesStart + 1, connectionStringEnd - quotesStart - 1);

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Errore: Connection string vuota");
    return;
}

Console.WriteLine($"Connessione a: {connectionString}");

var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new DashboardOrdersDbContext(options);

var nullAddressCustomers = await context.Customers
    .Where(c => c.Address == null)
    .ToListAsync();

Console.WriteLine($"Clienti con Address null: {nullAddressCustomers.Count}");

var validCustomers = await context.Customers
    .Where(c => c.Address != null)
    .ToListAsync();

Console.WriteLine($"Clienti con indirizzo valido: {validCustomers.Count}");

if (!validCustomers.Any())
{
    Console.WriteLine("Nessun cliente con indirizzo valido trovato per il campionamento.");
    return;
}

var rng = new Random();
foreach (var customer in nullAddressCustomers)
{
    var randomValidCustomer = validCustomers[rng.Next(validCustomers.Count)];
    customer.Address = randomValidCustomer.Address;
    Console.WriteLine($"Aggiorno ID {customer.Id}: {customer.Address}");
}

if (nullAddressCustomers.Any())
{
    await context.SaveChangesAsync();
    Console.WriteLine("\nOperazione completata: " + nullAddressCustomers.Count + " indirizzi null aggiornati con successo.");
}
else
{
    Console.WriteLine("Nessun indirizzo null da aggiornare.");
}
