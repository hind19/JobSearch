using System.Reflection;
using JobSearch.AI;
using JobSearch.Business;
using JobSearch.Email;
using JobSearch.Persistence;
using JobSearch.Scraping;
using JobSearch.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Mirrors JobSearch.WPF/App.xaml.cs: same appsettings.json / user-secrets /
// env var wiring, so both hosts read identical configuration.
// NOTE: Host.CreateApplicationBuilder only auto-loads user-secrets when
// EnvironmentName == "Development". Worker will typically run under
// Production (unattended via Task Scheduler), so user-secrets are added
// explicitly here — unlike App.xaml.cs, which can rely on the WPF debug
// environment defaulting to Development.
builder.Configuration.AddUserSecrets(
    Assembly.GetExecutingAssembly(), optional: true);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddBusinessServices()
    .AddAiServices(builder.Configuration)
    .AddEmailServices(builder.Configuration)   // ASSUMPTION: signature not verified — file wasn't shared
    .AddScrapingServices();

builder.Services.AddScoped<WorkerRun>();

// ADR-0001: single-run process. No AddHostedService/host.Run() — those are
// for long-running services. Build the host, run the pipeline once via
// WorkerRun, then exit with a status code for the external scheduler
// (Task Scheduler / cron) to observe.
using var host = builder.Build();

// One scope for the whole run: DbContext + WorkerRun (and everything it
// depends on) resolve from the same scope, avoiding root-container
// resolution of scoped services.
using var scope = host.Services.CreateScope();

// Same as App.xaml.cs: ensure the SQLite schema exists before any
// repository call. Directory creation for the (now absolute) DB path is
// handled centrally in PersistenceServiceExtensions.AddPersistence, so
// both hosts get it for free without duplicating the logic.
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.EnsureCreatedAsync();

var run = scope.ServiceProvider.GetRequiredService<WorkerRun>();
var exitCode = await run.ExecuteAsync(CancellationToken.None);

return exitCode;
