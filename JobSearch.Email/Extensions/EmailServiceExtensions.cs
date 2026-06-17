using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearch.Email;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));

        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton<EmailTemplateBuilder>();

        return services;
    }
}