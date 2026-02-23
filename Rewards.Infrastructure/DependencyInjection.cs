using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Rewards.Infrastructure.Persistence;

namespace Rewards.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRewardsInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RewardsDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "rewards")
                )
            );

            return services;
        }
    }
}
