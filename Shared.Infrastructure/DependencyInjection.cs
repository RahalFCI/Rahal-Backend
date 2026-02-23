using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Users.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAllModulesInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var extensionMethods = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsAbstract && t.IsSealed && t.IsClass) // static classes
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m =>
                    m.GetParameters().Length == 2 &&
                    m.GetParameters()[0].ParameterType == typeof(IServiceCollection) &&
                    m.GetParameters()[1].ParameterType == typeof(IConfiguration) &&
                    m.ReturnType == typeof(IServiceCollection));

            foreach (var method in extensionMethods)
            {
                method.Invoke(null, [services, configuration]);
            }

            return services;
        }
    }
}
