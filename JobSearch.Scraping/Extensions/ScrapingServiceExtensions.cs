using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.Scraping;

public static class ScrapingServiceExtensions
{
    public static IServiceCollection AddScrapingServices(
        this IServiceCollection services)
    {
        services.AddHttpClient<JobPageFetcher>();
        services.AddScoped<JobLinksScraper>();

        return services;
    }
}