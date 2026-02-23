using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Places.Infrastructure.Persistence;

namespace Places.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPlacesInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PlacesDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "places")
                )
            );

            return services;
        }
    }
}
