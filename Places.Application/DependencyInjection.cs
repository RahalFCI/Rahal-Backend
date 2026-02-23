using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPlacesApplication(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
