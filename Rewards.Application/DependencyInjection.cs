using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rewards.Application.Interfaces;
using Rewards.Application.Services;
using Rewards.Application.Validators;

namespace Rewards.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRewardsApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssemblyContaining<CreateCouponDtoValidator>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IUserCouponService, UserCouponService>();
            services.AddScoped<IPlanTierService, PlanTierService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ITravelPlanService, TravelPlanService>();

            return services;
        }
    }
}
