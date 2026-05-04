using FluentValidation;
using Gamification.Application.Strategies;
using Gamification.Application.Strategies.Implementations;
using Gamification.Application.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
