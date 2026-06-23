using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialMedia.Application.DTOs.Media;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Services;
using SocialMedia.Application.Validators;

namespace SocialMedia.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSocialMediaApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Media service
            services.AddScoped<IMediaService, MediaService>();

            // Post service
            services.AddScoped<IPostService, PostService>();

            // Validators
            services.AddScoped<IValidator<GenerateUploadSignaturesRequest>, GenerateUploadSignaturesRequestValidator>();

            return services;
        }
    }
}
