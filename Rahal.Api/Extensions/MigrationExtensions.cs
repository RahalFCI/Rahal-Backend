using Gamification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Places.Infrastructure.Persistence;
using Rewards.Infrastructure.Persistence;
using Shared.Application.Interfaces;
using SocialMedia.Infrastructure.Persistence;
using System.Data;
using Users.Infrastructure.Persistence;

namespace Rahal.Api.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            await MigrateAsync<UsersDbContext>(scope);
            await MigrateAsync<SocialMediaDbContext>(scope);
            await MigrateAsync<RewardsDbContext>(scope);
            await MigrateAsync<PlacesDbContext>(scope);
            await MigrateAsync<PaymentDbContext>(scope);
            await MigrateAsync<GamificationDbContext>(scope);

            var seeders = scope.ServiceProvider.GetServices<IDbInitializer>();

            try
            {
                foreach (var seeder in seeders)
                {
                    await seeder.SeedAsync();
                }
            }
            catch (Exception)
            {

            }
                
        }

        private static async Task MigrateAsync<TContext>(IServiceScope scope)
            where TContext : DbContext
        {
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            await context.Database.MigrateAsync();
        }

        public static async Task<WebApplication> SeedAsync(this WebApplication app)
        {
            
            return app;
        }
    }
}
