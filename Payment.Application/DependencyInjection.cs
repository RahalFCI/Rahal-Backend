using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Interfaces;
using Payment.Application.Services;

namespace Payment.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();

            return services;
        }
    }
}
