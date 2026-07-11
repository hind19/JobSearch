using System.IO;
using JobSearch.AI;
using JobSearch.Business;
using JobSearch.Email;
using JobSearch.Persistence;
using JobSearch.Scraping;
using JobSearch.WPF.Localization;
using JobSearch.WPF.ViewModels;
using JobSearch.WPF.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace JobSearch.WPF;

public partial class App : System.Windows.Application
{
    private IHost _host = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LocalizationManager.ApplyCurrentCulture();

        // Prevent WPF from shutting down when the login window closes (zero open windows).
        // We call Shutdown() explicitly: on cancel, and when the main window closes.
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(
                        "appsettings.json",
                        optional: false,
                        reloadOnChange: true)
                    .AddUserSecrets<App>(optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services
                    .AddPersistence(configuration)
                    .AddBusinessServices()
                    .AddAiServices(configuration)
                    .AddEmailServices(configuration)
                    .AddScrapingServices()
                    .AddWpfServices();
            })
            .Build();

        await _host.StartAsync();

        EnsureDatabaseDirectoryExists();

        await using var scope = _host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        if (loginWindow.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();

        if (loginWindow.ViewModel.LoggedInUserId.HasValue)
            await mainWindow.ViewModel.InitializeAsync(
                loginWindow.ViewModel.LoggedInUserId.Value);

        mainWindow.Closed += (_, _) => Shutdown();
        mainWindow.Show();
    }

    private void EnsureDatabaseDirectoryExists()
    {
        var connectionString = _host.Services
            .GetRequiredService<IConfiguration>()
            .GetConnectionString("DefaultConnection");

        if (connectionString is null) return;

        var dataSource = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .FirstOrDefault(p => p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            ?["Data Source=".Length..];

        if (string.IsNullOrWhiteSpace(dataSource) || Path.IsPathRooted(dataSource)) return;

        var dir = Path.GetDirectoryName(Path.Combine(AppContext.BaseDirectory, dataSource));
        if (dir is not null)
            Directory.CreateDirectory(dir);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(
                TimeSpan.FromSeconds(3));
        }

        base.OnExit(e);
    }
}
