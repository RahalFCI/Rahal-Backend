using Gamification.Application.Interfaces;
using Gamification.Application.Jobs;
using Gamification.Infrastructure.Persistence;
using Gamification.Infrastructure.Repositories;
using Gamification.Infrastructure.Search.Explorer;
using Gamification.Infrastructure.Search.Vendor;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;

namespace Gamification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGamificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionstringtemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionstring = connectionstringtemplate.Replace("$DATABASE_HOST", Environment.GetEnvironmentVariable("DATABASE_HOST"))
                .Replace("$DATABASE_PORT", Environment.GetEnvironmentVariable("DATABASE_PORT"))
                .Replace("$DATABASE_NAME", Environment.GetEnvironmentVariable("DATABASE_NAME"))
                .Replace("$DATABASE_USERNAME", Environment.GetEnvironmentVariable("DATABASE_USERNAME"))
                .Replace("$DATABASE_PASSWORD", Environment.GetEnvironmentVariable("DATABASE_PASSWORD"));

            services.AddDbContext<GamificationDbContext>(options =>
                options.UseNpgsql(
                    connectionstring,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "gamification")
                )
            );

            services.AddScoped(typeof(IGenericRepository<>), typeof(GamificationRepository<>));


            // Register Search Index Configuration
            services.AddScoped<ISearchIndexInitializer, ExplorerIndexConfig>();
            services.AddScoped<ISearchIndexInitializer, VendorIndexConfig>();

            //Register Hangfire for background jobs
            services.AddHangfire(config => config
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionstring);
                }));

            services.AddHangfireServer();
            services.AddScoped<StreakResetBackgroundJob>();

            //Register for gamification unit of work
            services.AddScoped<IGamificationUnitOfWork, GamificationUnitOfWork>();

            return services;
        }
    }
}
