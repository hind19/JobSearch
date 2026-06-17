using JobSearch.WPF.ViewModels;
using JobSearch.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.WPF;

public static class WpfServiceExtensions
{
    public static IServiceCollection AddWpfServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IServiceScopeFactory>(
            sp => sp.GetRequiredService<IServiceScopeFactory>());

        services.AddTransient<MainWindow>();
        services.AddTransient<UserProfileViewModel>();

        return services;
    }
}