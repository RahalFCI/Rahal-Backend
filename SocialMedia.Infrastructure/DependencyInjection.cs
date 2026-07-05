using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialMedia.Application.Interfaces;
using SocialMedia.Infrastructure.Persistence;
using SocialMedia.Infrastructure.Repositories;

namespace SocialMedia.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSocialMediaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionStringTemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionString = connectionStringTemplate
                .Replace("$DATABASE_HOST", Environment.GetEnvironmentVariable("DATABASE_HOST"))
                .Replace("$DATABASE_PORT", Environment.GetEnvironmentVariable("DATABASE_PORT"))
                .Replace("$DATABASE_NAME", Environment.GetEnvironmentVariable("DATABASE_NAME"))
                .Replace("$DATABASE_USERNAME", Environment.GetEnvironmentVariable("DATABASE_USERNAME"))
                .Replace("$DATABASE_PASSWORD", Environment.GetEnvironmentVariable("DATABASE_PASSWORD"));

            services.AddDbContext<SocialMediaDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "socialmedia")
                )
            );

            // Generic repository for BaseEntity-based entities (Post, Comment)
            services.AddScoped(typeof(ISocialMediaRepository<>), typeof(SocialMediaRepository<>));

            // Dedicated repositories for junction tables (composite PKs)
            services.AddScoped<IFollowRepository, FollowRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IPostPlaceRepository, PostPlaceRepository>();
            services.AddScoped<IUserGateway, UserGateway>();

            return services;
        }
    }
}
