using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rewards.Application.Interfaces;
using Rewards.Infrastructure.Persistence;
using Rewards.Infrastructure.Repositories;
using Rewards.Infrastructure.Search;
using Rewards.Infrastructure.Services;
using Shared.Application.Interfaces;

namespace Rewards.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRewardsInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionstringtemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionstring = connectionstringtemplate.Replace("$DATABASE_HOST", Environment.GetEnvironmentVariable("DATABASE_HOST"))
                .Replace("$DATABASE_PORT", Environment.GetEnvironmentVariable("DATABASE_PORT"))
                .Replace("$DATABASE_NAME", Environment.GetEnvironmentVariable("DATABASE_NAME"))
                .Replace("$DATABASE_USERNAME", Environment.GetEnvironmentVariable("DATABASE_USERNAME"))
                .Replace("$DATABASE_PASSWORD", Environment.GetEnvironmentVariable("DATABASE_PASSWORD"));

            services.AddDbContext<RewardsDbContext>(options =>
                options.UseNpgsql(
                    connectionstring,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "rewards")
                )
            );

            services.AddScoped(typeof(IRewardsRepository<>), typeof(RewardsRepository<>));
            services.AddScoped<ISearchIndexInitializer, CouponIndexConfig>();
            services.AddScoped<IRewardsGamificationService, RewardsGamificationService>();
            services.AddScoped<IRewardsPaymentService, RewardsPaymentService>();
            services.AddScoped<ICouponSearchService, CouponSearchService>();
            services.AddScoped<IRewardsUnitOfWork, RewardsUnitOfWork>();
            services.AddHttpClient<IRagTravelPlanService, RagTravelPlanService>(client =>
            {
                var baseUrl = configuration["AiSystem:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}
