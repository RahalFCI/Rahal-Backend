using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Places.Application.DTOs.CheckIn;
using Places.Application.DTOs.Place;
using Places.Application.DTOs.PlaceCategory;
using Places.Application.DTOs.PlaceReview;
using Places.Application.Interfaces;
using Places.Application.Services;
using Places.Application.Validators;

namespace Places.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPlacesApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssemblyContaining<CreatePlaceDtoValidator>();

            services.AddScoped<IValidator<CreatePlaceDto>, CreatePlaceDtoValidator>();
            services.AddScoped<IValidator<UpdatePlaceDto>, UpdatePlaceDtoValidator>();
            services.AddScoped<IValidator<CreatePlaceCategoryDto>, CreatePlaceCategoryDtoValidator>();
            services.AddScoped<IValidator<UpdatePlaceCategoryDto>, UpdatePlaceCategoryDtoValidator>();
            services.AddScoped<IValidator<CreateCheckInDto>, CreateCheckInDtoValidator>();
            services.AddScoped<IValidator<CreatePlaceReviewDto>, CreatePlaceReviewDtoValidator>();
            services.AddScoped<IValidator<UpdatePlaceReviewDto>, UpdatePlaceReviewDtoValidator>();
            services.AddScoped<IValidator<AddressDto>, AddressDtoValidator>();

            services.AddScoped<IPlaceCategoryService, PlaceCategoryService>();
            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<ICheckInService, CheckInService>();
            services.AddScoped<IPlacePhotoService, PlacePhotoService>();
            services.AddScoped<IPlaceReviewService, PlaceReviewService>();

            return services;
        }
    }
}
