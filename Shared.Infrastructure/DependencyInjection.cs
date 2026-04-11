using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Shared.Application.Interfaces;
using Shared.Application.Settings;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.FileStorage;
using Shared.Infrastructure.Repositories;
using Shared.Infrastructure.Resilience;
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
            services.Configure<ResilienceSettings>(configuration.GetSection(ResilienceSettings.SectionName));

            // Register ResiliencePipeline as singleton
            services.AddSingleton<ResiliencePipeline>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SearchResiliencePipelineFactory>>();
                var options = provider.GetRequiredService<IOptions<ResilienceSettings>>().Value;
                var factory = new SearchResiliencePipelineFactory(logger, options);
                return factory.CreatePipeline();
            });

            return services;
        }
 
    }
}

