using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.Interfaces;
using JobSearch.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JobSearch.Persistence;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString),
            ServiceLifetime.Scoped);

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserJobMatchRepository,
            UserJobMatchRepository>();
        services.AddScoped<IJobSiteRepository, JobSiteRepository>();

        return services;
    }
}