using JobSearch.AI;
using JobSearch.Business;
using JobSearch.Email;
using JobSearch.Persistence;
using JobSearch.Scraping;
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

        await using var scope = _host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        var mainWindow = _host.Services
            .GetRequiredService<MainWindow>();

        mainWindow.Show();
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
