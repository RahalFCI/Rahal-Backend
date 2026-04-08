using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            var host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis";
            var port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

            //Configure Redis Connection
            var redisConnectionTemplate = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is required");

            var redisConnection = redisConnectionTemplate
                .Replace("$REDIS_HOST", host)
                .Replace("$REDIS_PORT", port);

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var _redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
                var redisConfig = ConfigurationOptions.Parse(redisConnection);
                redisConfig.AbortOnConnectFail = _redisSettings.AbortOnConnectFail;
                redisConfig.ConnectRetry = _redisSettings.ConnectRetry;
                redisConfig.ConnectTimeout = _redisSettings.ConnectTimeout;
                return ConnectionMultiplexer.Connect(redisConfig);


            });

            services.AddScoped<ICacheService, RedisCacheService>(); //Internally redis registers cache as singleton, injected as scoped to avoid issues with HttpContext access
            services.AddScoped<ICurrentUserService, CurrentUserService>();


            return services;
        }
    }
}
