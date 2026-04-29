using System.Reflection;
using DashboardOrders.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);
builder.Logging.AddDashboardLogging();

builder.Services.AddControllersWithViews();
builder.Services.AddDashboardApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (await app.TryRunDashboardCommandAsync(args))
{
    return;
}

app.UseDashboardRequestPipeline();

app.Run();
