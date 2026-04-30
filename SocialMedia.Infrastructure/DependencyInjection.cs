using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMedia.Infrastructure.Persistence;

namespace SocialMedia.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSocialMediaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionstringtemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionstring = connectionstringtemplate.Replace("$DATABASE_HOST", Environment.GetEnvironmentVariable("DATABASE_HOST"))
                .Replace("$DATABASE_PORT", Environment.GetEnvironmentVariable("DATABASE_PORT"))
                .Replace("$DATABASE_NAME", Environment.GetEnvironmentVariable("DATABASE_NAME"))
                .Replace("$DATABASE_USERNAME", Environment.GetEnvironmentVariable("DATABASE_USERNAME"))
                .Replace("$DATABASE_PASSWORD", Environment.GetEnvironmentVariable("DATABASE_PASSWORD"));

            services.AddDbContext<SocialMediaDbContext>(options =>
                options.UseNpgsql(
                    connectionstring,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "socialmedia")
                )
            );

            return services;
        }
    }
}
