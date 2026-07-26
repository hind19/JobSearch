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

            // Additional headers a real Chrome request always sends —
            // some bot-detection systems flag requests missing these,
            // even with a valid User-Agent. Cheap to add; not a
            // guaranteed fix for anything past basic header inspection
            // (TLS/HTTP2 fingerprinting and JS challenges are a
            // different, much harder problem — see chat discussion on
            // robota.ua/Cloudflare).
            client.DefaultRequestHeaders.Accept.ParseAdd(
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
                "en-US,en;q=0.9,ru;q=0.8,uk;q=0.7");

            client.Timeout = TimeSpan.FromSeconds(30);
        })
        // Required because Accept-Encoding is advertised above — without
        // this, a server that actually compresses its response leaves
        // HttpClient reading raw gzip bytes as if they were UTF-8 text.
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip
                | System.Net.DecompressionMethods.Deflate
        });

        services.AddScoped<IJobLinksScraper, JobLinksScraper>();

        return services;
    }
}