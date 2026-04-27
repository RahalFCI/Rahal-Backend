using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Places.Application.DTOs.Place;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;

namespace Places.Application.Services
{
    internal class PlaceService : IPlaceService
    {
        private readonly IGenericRepository<Place> _placeRepository;
        private readonly IGenericRepository<PlaceCategory> _categoryRepository;
        private readonly ILogger<PlaceService> _logger;

        public PlaceService(
            IGenericRepository<Place> placeRepository,
            IGenericRepository<PlaceCategory> categoryRepository,
            ILogger<PlaceService> logger)
        {
            _placeRepository = placeRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetPlaceDto>> GetPlaceByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching place {PlaceId}", id);

            var place = await _placeRepository.GetByIdAsync(id, cancellationToken);

            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", id);
                return ApiResponse<GetPlaceDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetPlaceDto>.Success(PlaceMapper.ToGetDto(place));
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceDto>>> GetAllPlacesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all places");

            var places = await _placeRepository.GetTable().Include(p => p.PlaceCategory).ToListAsync(cancellationToken);
            var dtos = PlaceMapper.ToGetDtos(places);

            _logger.LogInformation("Retrieved {PlaceCount} places", places.Count());

            return ApiResponse<IEnumerable<GetPlaceDto>>.Success(dtos);
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceDto>>> GetPlacesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching places for category {CategoryId}", categoryId);

            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", categoryId);
                return ApiResponse<IEnumerable<GetPlaceDto>>.Failure(ErrorCode.NotFound);
            }

            var places = await _placeRepository.GetTable().Where(p => p.PlaceCategoryId == categoryId).Include(p => p.PlaceCategory).ToListAsync(cancellationToken);
            var dtos = PlaceMapper.ToGetDtos(places);

            _logger.LogInformation("Retrieved {PlaceCount} places for category {CategoryId}", places.Count(), categoryId);

            return ApiResponse<IEnumerable<GetPlaceDto>>.Success(dtos);
        }

        public async Task<ApiResponse<string>> CreatePlaceAsync(CreatePlaceDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new place");

            var category = await _categoryRepository.GetByIdAsync(dto.PlaceCategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", dto.PlaceCategoryId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var place = PlaceMapper.ToEntity(dto);
            _placeRepository.Add(place);
            await _placeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Place created successfully with ID {PlaceId}", place.Id);

            return ApiResponse<string>.Success($"Place created successfully. ID: {place.Id}");
        }

        public async Task<ApiResponse<string>> UpdatePlaceAsync(Guid id, UpdatePlaceDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating place {PlaceId}", id);

            var place = await _placeRepository.GetByIdAsync(id, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var category = await _categoryRepository.GetByIdAsync(dto.PlaceCategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", dto.PlaceCategoryId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            PlaceMapper.UpdateEntity(place, dto);
            await _placeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Place {PlaceId} updated successfully", id);

            return ApiResponse<string>.Success("Place updated successfully");
        }

        public async Task<ApiResponse<string>> DeletePlaceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting place {PlaceId}", id);

            var placeExists = await _placeRepository.GetTable().AnyAsync(e => e.Id == id, cancellationToken);
            if (!placeExists)
            {
                _logger.LogWarning("Place {PlaceId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Place place = new Place() { Id = id, IsDeleted = true };
            _placeRepository.SaveInclude(place, nameof(place.IsDeleted));
            await _placeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Place {PlaceId} deleted successfully", id);

            return ApiResponse<string>.Success("Place deleted successfully");
        }

        public async Task<ApiResponse<string>> DeletePlacePermanentlyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Permanently deleting place {PlaceId}", id);

            var place = await _placeRepository.GetByIdAsync(id, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found for permanent deletion", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _placeRepository.Delete(place);
            await _placeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Place {PlaceId} permanently deleted", id);

            return ApiResponse<string>.Success("Place permanently deleted");
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceDto>>> SearchPlacesByLocationAsync(double latitude, double longitude, int radiusInMeters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching places near latitude {Latitude}, longitude {Longitude} with radius {Radius} meters", 
                latitude, longitude, radiusInMeters);

            var places = await _placeRepository.GetTable().Include(p => p.PlaceCategory).ToListAsync(cancellationToken);

            var nearbyPlaces = places.Where(p =>
                CalculateDistance(p.Latitude, p.Longitude, latitude, longitude) <= radiusInMeters);

            var dtos = PlaceMapper.ToGetDtos(nearbyPlaces);

            _logger.LogInformation("Found {PlaceCount} places within {Radius} meters", nearbyPlaces.Count(), radiusInMeters);

            return ApiResponse<IEnumerable<GetPlaceDto>>.Success(dtos);
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Earth's radius in meters
            const double R = 6371000;

            // Convert coordinate differences from degrees to radians
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            // Haversine formula:
            // a = sin²(Δlat/2) + cos(lat1) * cos(lat2) * sin²(Δlon/2)
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            // c = 2 * atan2(√a, √(1-a)) — angular distance in radians
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Multiply angular distance by Earth's radius to get meters
            return R * c;
        }
    }
}
