using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Admin;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.Register;
using Users.Application.DTOs.Vendor;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Application.Services;
using Users.Application.Settings;
using Users.Application.Validators;
using Users.Domain.Entities;

namespace Users.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersApplication(this IServiceCollection services, IConfiguration configuration)
        {
            //Configure JWT Settings
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            services.Configure<JWTSettings>(options =>
            {
                configuration.GetSection("JwtSettings").Bind(options);

                // Override the secret with environment variable
                options.Secret = secretKey;
            });
            var jwtSettings = configuration
                .GetSection("JwtSettings")
                .Get<JWTSettings>();
            if (jwtSettings != null) jwtSettings.Secret = secretKey;


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //How Asp.Net will authenticate the user
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //How will Asp.Net will respond when authentication fails
            })
                .AddJwtBearer(op =>
                {
                    op.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,             // Check that token's issuer matches our expected Issuer
                        ValidateAudience = true,           // Check that token's audience matches our expected Audience
                        ValidateLifetime = true,           // Ensure token hasn't expired
                        ValidIssuer = jwtSettings!.Issuer, // Our configured issuer (from appsettings.json or env vars)
                        ValidAudience = jwtSettings.Audience, // Our configured audience
                        ValidateIssuerSigningKey = true,   // Check token signature
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret!)) // Use our secret key to verify signature
                    };
                });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); //Require User to be Authenticcated for all endpoints by default
            });

            // Register Fluent Validation
            services.AddValidatorsFromAssemblyContaining<RegisterExplorerDtoValidator>();
            services.AddScoped<IValidator<RegisterExplorerDto>, RegisterExplorerDtoValidator>();
            services.AddScoped<IValidator<RegisterVendorDto>, RegisterVendorDtoValidator>();
            services.AddScoped<IValidator<RegisterAdminDto>, RegisterAdminDtoValidator>();
            services.AddScoped<IValidator<AuthRequestDto>, AuthRequestDtoValidator>();
            services.AddScoped<IValidator<UpdatePasswordDto>, UpdatePasswordDtoValidator>();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register Mappers and Services
            services.AddScoped<IUserMapper<Vendor, VendorDto, VendorSummaryDto>, VendorMapper>();
            services.AddScoped<IUserMapper<Explorer, ExplorerDto, ExplorerSummaryDto>, ExplorerMapper>();
            services.AddScoped<IUserMapper<Admin, AdminDto, AdminSummaryDto>, AdminMapper>();

            services.AddScoped<IUserService<Vendor, VendorDto, VendorSummaryDto>,
                UserService<Vendor, VendorDto, VendorSummaryDto>>();
            services.AddScoped<IUserService<Admin, AdminDto, AdminSummaryDto>,
                UserService<Admin, AdminDto, AdminSummaryDto>>();
            services.AddScoped<IUserService<Explorer, ExplorerDto, ExplorerSummaryDto>,
                UserService<Explorer, ExplorerDto, ExplorerSummaryDto>>();
            services.AddScoped<IAuthService<Explorer>, AuthService<Explorer>>();
            services.AddScoped<IAuthService<Vendor>, AuthService<Vendor>>();
            services.AddScoped<IAuthService<Admin>, AuthService<Admin>>();

            return services;
        }
    }
}
