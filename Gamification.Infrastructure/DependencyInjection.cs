using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGamificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
