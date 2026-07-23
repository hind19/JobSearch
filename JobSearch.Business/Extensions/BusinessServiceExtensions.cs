using JobSearch.Application.Abstractions;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.Business;

public static class BusinessServiceExtensions
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services)
    {
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobMatchService, JobMatchService>();
        services.AddScoped<IJobIngestService, JobIngestService>();
        services.AddSingleton<IJobUrlHasher, JobUrlHasher>();
        services.AddScoped<IJobSiteService, JobSiteService>();
        // ISP split: Worker depends on the narrow IJobSiteQueryService,
        // WPF depends on the full IJobSiteService. Both resolve to the
        // same JobSiteService instance within a scope.
        services.AddScoped<IJobSiteQueryService>(
            sp => sp.GetRequiredService<IJobSiteService>());

        return services;
    }
}