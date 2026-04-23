using Meilisearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Shared.Application.Interfaces;
using Shared.Application.Settings;
using Shared.Application.Settings.ReslilienceSettings;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.FileStorage;
using Shared.Infrastructure.Repositories;
using Shared.Infrastructure.Resilience;
using Shared.Infrastructure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailSettings>(
            configuration.GetSection(MailSettings.SectionName));

            services.AddTransient<IEmailService, SmtpEmailService>();

            // Register file storage service
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // Configure resilience options from appsettings
            services.Configure<SearchResilienceSettings>(configuration.GetSection(SearchResilienceSettings.SectionName));
            services.Configure<EmailResilienceSettings>(configuration.GetSection(EmailResilienceSettings.SectionName));
            services.Configure<FileStorageResilienceSettings>(configuration.GetSection(FileStorageResilienceSettings.SectionName));

            // Register ResiliencePipeline as singleton
            services.AddSingleton<SearchResiliencePipelineFactory>();
            services.AddSingleton<EmailResiliencePipelineFactory>();
            services.AddSingleton<FileStorageResiliencePipelineFactory>();

            services.AddResiliencePipeline("search", (builder, context) =>
            {
                var factory = context.ServiceProvider.GetRequiredService<SearchResiliencePipelineFactory>();
                var pipeline = factory.CreatePipeline();

                // copy strategies from factory pipeline into the builder
                builder.AddPipeline(pipeline);
            });

            services.AddResiliencePipeline("email", (builder, context) =>
            {
                var factory = context.ServiceProvider.GetRequiredService<EmailResiliencePipelineFactory>();
                var pipeline = factory.CreatePipeline();

                // copy strategies from factory pipeline into the builder
                builder.AddPipeline(pipeline);
            });

            services.AddResiliencePipeline("file-storage", (builder, context) =>
            {
                var factory = context.ServiceProvider.GetRequiredService<FileStorageResiliencePipelineFactory>();
                var pipeline = factory.CreatePipeline();

                // copy strategies from factory pipeline into the builder
                builder.AddPipeline(pipeline);
            });


            // Configure Meilisearch settings
            services.Configure<MeilisearchSettings>(configuration.GetSection(MeilisearchSettings.SectionName));

            // Register MeilisearchClient as singleton
            services.AddSingleton<MeilisearchClient>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<MeilisearchSettings>>().Value;
                var logger = provider.GetRequiredService<ILogger<MeilisearchClient>>();

                // Get environment variables for Meilisearch host and port
                var meiliHost = Environment.GetEnvironmentVariable("MEILI_HOST") ?? "localhost";
                var meiliPort = Environment.GetEnvironmentVariable("MEILI_PORT") ?? "7700";
                var meiliApiKey = Environment.GetEnvironmentVariable("MEILI_API_KEY") ?? "masterKey123";

                // Replace template variables in the URL
                var meilisearchUrl = settings.Url
                    .Replace("$MEILI_HOST", meiliHost)
                    .Replace("$MEILI_PORT", meiliPort);

                settings.ApiKey = meiliApiKey;

                if (!settings.IsValid())
                {
                    logger.LogWarning(
                        "Meilisearch settings are not configured properly. Url: {IsUrlValid}, ApiKey: {IsApiKeyValid}",
                        !string.IsNullOrWhiteSpace(meilisearchUrl),
                        !string.IsNullOrWhiteSpace(settings.ApiKey));
                }

                return new MeilisearchClient(meilisearchUrl, settings.ApiKey);
            });

            // Register MeilisearchService as open generic
            services.AddScoped(typeof(ISearchService<>), typeof(MeilisearchService<>));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


            return services;
        }

    }
}

