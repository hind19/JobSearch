using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.Scraping;

public static class ScrapingServiceExtensions
{
    public static IServiceCollection AddScrapingServices(
        this IServiceCollection services)
    {
        services.AddHttpClient<JobLinksScraper>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IJobLinksScraper, JobLinksScraper>();

        return services;
    }
}