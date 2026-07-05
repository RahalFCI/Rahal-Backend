using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Interfaces;
using Notifications.Application.Services;

namespace Notifications.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationsApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
