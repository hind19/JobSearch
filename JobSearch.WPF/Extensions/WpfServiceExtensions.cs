using JobSearch.WPF.Dialogs;
using JobSearch.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.WPF;

public static class WpfServiceExtensions
{
    public static IServiceCollection AddWpfServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IServiceScopeFactory>(
            sp => sp.GetRequiredService<IServiceScopeFactory>());
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<MainWindow>();
        services.AddTransient<UserProfileViewModel>();

        return services;
    }
}