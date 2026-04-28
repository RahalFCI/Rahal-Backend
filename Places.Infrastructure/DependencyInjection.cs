using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Places.Infrastructure.Persistence;
using Places.Infrastructure.Search;
using Places.Infrastructure.Search.EventHandlers;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Repositories;

namespace Places.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPlacesInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionstringtemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionstring = connectionstringtemplate.Replace("$DATABASE_HOST", Environment.GetEnvironmentVariable("DATABASE_HOST"))
                .Replace("$DATABASE_PORT", Environment.GetEnvironmentVariable("DATABASE_PORT"))
                .Replace("$DATABASE_NAME", Environment.GetEnvironmentVariable("DATABASE_NAME"))
                .Replace("$DATABASE_USERNAME", Environment.GetEnvironmentVariable("DATABASE_USERNAME"))
                .Replace("$DATABASE_PASSWORD", Environment.GetEnvironmentVariable("DATABASE_PASSWORD"));

            services.AddDbContext<PlacesDbContext>(options =>
                options.UseNpgsql(
                    connectionstring,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "places")
                )
            );

            services.AddScoped<DbContext, PlacesDbContext>();

            services.AddScoped<IDbInitializer, PlacesDBInitializer>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<ISearchIndexInitializer, PlaceIndexConfig>();

            return services;
        }
    }
}
