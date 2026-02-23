using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMedia.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSocialMediaApplication(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
