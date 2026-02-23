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
            services.AddDbContext<SocialMediaDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "socialmedia")
                )
            );

            return services;
        }
    }
}
