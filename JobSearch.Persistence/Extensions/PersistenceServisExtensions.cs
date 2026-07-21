using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;

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

        // ADR: shared local SQLite DB lives in a fixed, absolute location
        // (%ProgramData%\JobSearch\Database\jobsearch.db) so JobSearch.WPF
        // and JobSearch.Worker always resolve to the same physical file,
        // regardless of each host's own working/bin directory or the
        // Windows account each process runs under. Expand here (not in
        // each host's Program.cs/App.xaml.cs) so both hosts get identical
        // behavior for free.
        connectionString = Environment.ExpandEnvironmentVariables(connectionString);

        EnsureDataSourceDirectoryExists(connectionString);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString),
            ServiceLifetime.Scoped);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserSkillRepository, UserSkillRepository>();
        services.AddScoped<IJobSiteRepository, JobSiteRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUserJobMatchRepository, UserJobMatchRepository>();

        return services;
    }

    private static void EnsureDataSourceDirectoryExists(string connectionString)
    {
        var dataSource = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .FirstOrDefault(p => p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            ?["Data Source=".Length..];

        if (string.IsNullOrWhiteSpace(dataSource)) return;

        // Relative paths are still supported (resolved against the
        // running process's base directory) for local/dev overrides, but
        // the standard config now uses an absolute path.
        var fullPath = Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(AppContext.BaseDirectory, dataSource);

        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }
}