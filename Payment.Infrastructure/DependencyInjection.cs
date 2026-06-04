using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using Shared.Application.Interfaces;

namespace Payment.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionstringtemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionstring = connectionstringtemplate.Replace("$DATABASE_HOST", Environment.GetEnvironmentVariable("DATABASE_HOST"))
                .Replace("$DATABASE_PORT", Environment.GetEnvironmentVariable("DATABASE_PORT"))
                .Replace("$DATABASE_NAME", Environment.GetEnvironmentVariable("DATABASE_NAME"))
                .Replace("$DATABASE_USERNAME", Environment.GetEnvironmentVariable("DATABASE_USERNAME"))
                .Replace("$DATABASE_PASSWORD", Environment.GetEnvironmentVariable("DATABASE_PASSWORD"));

            services.AddDbContext<PaymentDbContext>(options =>
                options.UseNpgsql(
                    connectionstring,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "payment")
                )
            );

            services.AddScoped(typeof(IGenericRepository<>), typeof(PaymentRepository<>));

            return services;
        }
    }
}
