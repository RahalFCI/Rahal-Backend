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
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.EmailVerification;
using Users.Application.DTOs.OAuth;
using Users.Application.DTOs.Register;
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

            services.AddValidatorsFromAssemblyContaining<AuthRequestDtoValidator>();

            services.AddScoped<IValidator<AuthRequestDto>, AuthRequestDtoValidator>();
            services.AddScoped<IValidator<UpdatePasswordDto>, UpdatePasswordDtoValidator>();
            services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
            services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
            services.AddScoped<IValidator<GoogleSignInRequest>, GoogleSignInRequestValidator>();
            services.AddScoped<IValidator<VerifyOtpRequest>, VerifyOtpValidator>();
            services.AddScoped<IValidator<ResendOtpRequest>, ResendOtpValidator>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IPasswordResetService, PasswordResetService>();

            services.AddScoped<IEmailVerificationService, EmailVerificationService>();

            services.AddScoped<IProfilePictureService, ProfilePictureService>();

            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddScoped<IOAuthGoogleService, GoogleAuthService>();
            services.AddScoped<IOAuthGoogleFacade, GoogleOAuthFacade>();

            services.AddScoped<ITokenService, TokenService>();


            return services;
        }
    }
}

