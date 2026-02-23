using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rewards.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRewardsInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
