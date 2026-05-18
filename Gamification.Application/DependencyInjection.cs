using FluentValidation;
using Gamification.Application.Interfaces;
using Gamification.Application.Strategies;
using Gamification.Application.Strategies.Implementations;
using Gamification.Application.Utils;
using Gamification.Application.Validators.Achievement;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;

namespace Gamification.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGamificationApplication(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddValidatorsFromAssembly(typeof(CreateAchievementDtoValidator).Assembly);

            services.AddScoped<XpCalculationStrategyResolver>();

            services.AddScoped<IXpCalculationStrategy, CheckInXpStrategy>();
            services.AddScoped<IXpCalculationStrategy, AchievementXpStrategy>();
            services.AddScoped<IXpCalculationStrategy, ChallengeXpStrategy>();
            services.AddScoped<IXpCalculationStrategy, SocialMediaXpStrategy>();
            services.AddScoped<IXpCalculationStrategy, BonusXpStrategy>();

            services.AddScoped<IProfileChecker, ProfileChecker>();

            



            return services;
        }
    }
}
