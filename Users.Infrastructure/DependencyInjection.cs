using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Search;
using Users.Infrastructure.Search.EventHandlers;

namespace Users.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionstringtemplate = configuration.GetConnectionString("DefaultConnection")!;

            // Get environment variables with proper null coalescing
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "Rahal";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "admin";

            string connectionstring = connectionstringtemplate
                .Replace("$DATABASE_HOST", host)
                .Replace("$DATABASE_PORT", port)
                .Replace("$DATABASE_NAME", database)
                .Replace("$DATABASE_USERNAME", username)
                .Replace("$DATABASE_PASSWORD", password);

            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(
                    connectionstring,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "users")
                )
            );

            // Enable Identity with default role type
            services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;

                //Password Complexity Configuration
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;
            })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<UsersDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<User, Role, UsersDbContext, Guid>>()
                .AddRoleStore<RoleStore<Role, UsersDbContext, Guid>>();

            services.AddScoped<SignInManager<User>>();


            services.AddScoped<IDbInitializer, DBInitializer>();

            // Register Email Verification Repository
            services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();

            // Register Search Index Configuration
            services.AddScoped<ISearchIndexInitializer, UserIndexConfig>();

            //Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}

