using JobSearch.WPF.Dialogs;
using JobSearch.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.WPF;

public static class WpfServiceExtensions
{
    public static IServiceCollection AddWpfServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<UserProfileViewModel>();
        services.AddTransient<JobSitesViewModel>();
        services.AddTransient<EmailSettingsViewModel>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<JobSitesViewModel>();

        return services;
    }
}