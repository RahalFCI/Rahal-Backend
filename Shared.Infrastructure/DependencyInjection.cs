using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Application.Settings;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.Repositories;
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

            return services;
        }
    }
}
