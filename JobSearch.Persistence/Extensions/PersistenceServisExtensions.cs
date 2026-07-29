using JobSearch.Persistence.Abstractions;
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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Раскрыть переменные окружения (%ProgramData% и т.д.)
        connectionString = Environment.ExpandEnvironmentVariables(connectionString);

        // Создать директорию базы данных, если не существует
        if (connectionString.Contains("Data Source="))
        {
            var dataSourceMatch = System.Text.RegularExpressions.Regex.Match(
                connectionString,
                @"Data Source\s*=\s*([^;]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (dataSourceMatch.Success)
            {
                var dbPath = dataSourceMatch.Groups[1].Value.Trim();
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString),
            ServiceLifetime.Scoped);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserSkillRepository, UserSkillRepository>();
        services.AddScoped<IJobSiteRepository, JobSiteRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUserJobMatchRepository, UserJobMatchRepository>();
        services.AddScoped<IUserJobRejectionRepository, UserJobRejectionRepository>(); // ADR-0009
        services.AddScoped<ISentEmailRepository, SentEmailRepository>();
        services.AddScoped<IEmailSettingsRepository, EmailSettingsRepository>();
        services.AddScoped<IJobStatisticsRepository, JobStatisticsRepository>();

        return services;
    }
}