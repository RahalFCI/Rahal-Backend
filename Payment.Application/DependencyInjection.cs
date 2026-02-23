using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentApplication(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
