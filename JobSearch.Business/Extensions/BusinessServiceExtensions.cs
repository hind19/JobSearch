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
        services.AddScoped<IJobSiteService, JobSiteService>();

        return services;
    }
}