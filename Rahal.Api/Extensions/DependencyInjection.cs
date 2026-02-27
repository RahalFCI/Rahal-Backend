using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gamification.Application;
using Gamification.Infrastructure;
using Payment.Application;
using Payment.Infrastructure;
using Places.Application;
using Places.Infrastructure;
using Rewards.Application;
using Rewards.Infrastructure;
using SocialMedia.Application;
using SocialMedia.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Users.Application;
using Users.Infrastructure;
using Shared.Application;

namespace Shared.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAllModules(this IServiceCollection services, IConfiguration configuration)
        {
            //Users Module
            services.AddUsersApplication(configuration);
            services.AddUsersInfrastructure(configuration);

            //Social Media Module
            services.AddSocialMediaApplication(configuration);
            services.AddSocialMediaInfrastructure(configuration);

            //Rewards Module
            services.AddRewardsApplication(configuration);
            services.AddRewardsInfrastructure(configuration);

            //Places Module
            services.AddPlacesApplication(configuration);
            services.AddPlacesInfrastructure(configuration);

            //Gamification Module
            services.AddGamificationApplication(configuration);
            services.AddGamificationInfrastructure(configuration);

            //Payment Module
            services.AddPaymentApplication(configuration);
            services.AddPaymentInfrastructure(configuration);
            
            //Shared Module
            services.AddSharedApplication(configuration);
            services.AddSharedInfrastructure(configuration);

            return services;
        }
    }
}
