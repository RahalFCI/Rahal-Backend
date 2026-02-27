using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Application.Services;
using Shared.Application.Settings;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind RedisSettings from environment variables
            var redisSettings = new RedisSettings
            {
                AbortOnConnectFail = bool.TryParse(Environment.GetEnvironmentVariable("RedisSettings__AbortOnConnectFail"), out var abort) ? abort : false,
                ConnectRetry = int.TryParse(Environment.GetEnvironmentVariable("RedisSettings__ConnectRetry"), out var retry) ? retry : 3,
                ConnectTimeout = int.TryParse(Environment.GetEnvironmentVariable("RedisSettings__ConnectTimeout"), out var timeout) ? timeout : 5000
            };

            // Configure Redis Connection
            var redisConnection = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is required");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfig = ConfigurationOptions.Parse(redisConnection);
                redisConfig.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
                redisConfig.ConnectRetry = redisSettings.ConnectRetry;
                redisConfig.ConnectTimeout = redisSettings.ConnectTimeout;
                return ConnectionMultiplexer.Connect(redisConfig);
            });

            services.AddScoped<ICacheService, RedisCacheService>(); //Internally redis registers cache as singleton, injected as scoped to avoid issues with HttpContext access

            return services;
        }
    }
}
