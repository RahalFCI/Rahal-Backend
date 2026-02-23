using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Infrastructure.Persistence;

namespace Gamification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGamificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GamificationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "gamification")
                )
            );

            return services;
        }
    }
}
