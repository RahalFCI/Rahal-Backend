using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Users.Application.DTOs.Admin;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.OAuth;
using Users.Application.DTOs.Register;
using Users.Application.DTOs.Vendor;
using Users.Application.Factory;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Application.Services;
using Users.Application.Settings;
using Users.Application.Validators;

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
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(op =>
                {
                    op.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtSettings!.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret!)),
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            services.AddAuthorization();

            // Register Fluent Validation
            services.AddValidatorsFromAssemblyContaining<RegisterExplorerDtoValidator>();

            // Register DTO Validators
            services.AddScoped<IValidator<RegisterExplorerDto>, RegisterExplorerDtoValidator>();
            services.AddScoped<IValidator<RegisterVendorDto>, RegisterVendorDtoValidator>();
            services.AddScoped<IValidator<RegisterAdminDto>, RegisterAdminDtoValidator>();
            services.AddScoped<IValidator<ExplorerDto>, ExplorerDtoValidator>();
            services.AddScoped<IValidator<VendorDto>, VendorDtoValidator>();
            services.AddScoped<IValidator<AdminDto>, AdminDtoValidator>();
            services.AddScoped<IValidator<AuthRequestDto>, AuthRequestDtoValidator>();
            services.AddScoped<IValidator<UpdatePasswordDto>, UpdatePasswordDtoValidator>();
            services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
            services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
            services.AddScoped<IValidator<GoogleSignInRequest>, GoogleSignInRequestValidator>();

            // Register Single Auth Service
            services.AddScoped<IAuthService, AuthService>();

            // Register Password Reset Service
            services.AddScoped<IPasswordResetService, PasswordResetService>();

            // Register Google OAuth Services
            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddScoped<IOAuthGoogleService, GoogleAuthService>();
            services.AddScoped<IOAuthGoogleFacade, GoogleOAuthFacade>();

            // Register Factories (now return User instead of specific types)
            services.AddScoped<IUserFactory<RegisterExplorerDto, Users.Domain.Entities._Common.User>, ExplorerUserFactory>();
            services.AddScoped<IUserFactory<RegisterVendorDto, Users.Domain.Entities._Common.User>, VendorUserFactory>();
            services.AddScoped<IUserFactory<RegisterAdminDto, Users.Domain.Entities._Common.User>, AdminUserFactory>();

            // Register Mappers
            services.AddScoped<IUserMapper<ExplorerDto, ExplorerSummaryDto>, ExplorerMapper>();
            services.AddScoped<IUserMapper<VendorDto, VendorSummaryDto>, VendorMapper>();
            services.AddScoped<IUserMapper<AdminDto, AdminSummaryDto>, AdminMapper>();

            // Register Type-Specific User Services
            services.AddScoped<IUserService<ExplorerDto, ExplorerSummaryDto>, ExplorerService>();
            services.AddScoped<IUserService<VendorDto, VendorSummaryDto>, VendorService>();
            services.AddScoped<IUserService<AdminDto, AdminSummaryDto>, AdminService>();

            // Register Token Service
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
