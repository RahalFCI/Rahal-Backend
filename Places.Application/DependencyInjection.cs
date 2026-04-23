using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Places.Application.Interfaces;
using Places.Application.Services;

namespace Places.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPlacesApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPlaceCategoryService, PlaceCategoryService>();
            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<ICheckInService, CheckInService>();
            services.AddScoped<IPlacePhotoService, PlacePhotoService>();
            services.AddScoped<IPlaceReviewService, PlaceReviewService>();

            return services;
        }
    }
}
