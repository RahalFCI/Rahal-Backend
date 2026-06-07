using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Application.DTOs.Place;
using Places.Application.Events;
using Places.Application.Helpers;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using Shared.Infrastructure.Pagination;

namespace Places.Application.Services
{
    internal class PlaceService : IPlaceService
    {
        private readonly IPlacesRepository<Place> _placeRepository;
        private readonly IPlacesRepository<PlaceCategory> _categoryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<PlaceService> _logger;

        public PlaceService(
            IPlacesRepository<Place> placeRepository,
            IPlacesRepository<PlaceCategory> categoryRepository,
            IMediator mediator,
            ILogger<PlaceService> logger)
        {
            _placeRepository = placeRepository;
            _categoryRepository = categoryRepository;
            _mediator = mediator;
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

        public async Task<ApiResponse<PagedResult<GetPlaceDto>>> GetAllPlacesAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all places - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var result = await _placeRepository.GetTable()
                .Select(p => PlaceMapper.ToGetDto(p))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<GetPlaceDto>>.Success(result);
        }

        public async Task<ApiResponse<PagedResult<GetPlaceDto>>> GetPlacesByCategoryIdAsync(Guid categoryId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching places for category {CategoryId} - page {Page}, pageSize {PageSize}", categoryId, request.Page, request.PageSize);

            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", categoryId);
                return ApiResponse<PagedResult<GetPlaceDto>>.Failure(ErrorCode.NotFound);
            }

            var result = await _placeRepository.GetTable()
                .Where(p => p.PlaceCategoryId == categoryId)
                .Select(p => PlaceMapper.ToGetDto(p))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<GetPlaceDto>>.Success(result);
        }

        public async Task<ApiResponse<Guid>> CreatePlaceAsync(CreatePlaceDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new place");

            var category = await _categoryRepository.GetByIdAsync(dto.PlaceCategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", dto.PlaceCategoryId);
                return ApiResponse<Guid>.Failure(ErrorCode.NotFound);
            }

            var place = PlaceMapper.ToEntity(dto);
            _placeRepository.Add(place);
            await _placeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Place created successfully with ID {PlaceId}", place.Id);

            try
            {
                await _mediator.Publish(new PlaceCreatedEvent(place.Id, place.Name, place.PlaceCategoryId), cancellationToken);
                _logger.LogInformation("PlaceCreatedEvent published for place {PlaceId}", place.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PlaceCreatedEvent for place {PlaceId}. Place creation was successful but search index may not be updated.", place.Id);
            }

            return ApiResponse<Guid>.Success(place.Id);
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

            try
            {
                await _mediator.Publish(new PlaceUpdatedEvent(place.Id, place.Name, place.PlaceCategoryId), cancellationToken);
                _logger.LogInformation("PlaceUpdatedEvent published for place {PlaceId}", place.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PlaceUpdatedEvent for place {PlaceId}. Place update was successful but search index may not be updated.", place.Id);
            }

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

            try
            {
                await _mediator.Publish(new PlaceDeletedEvent(place.Id), cancellationToken);
                _logger.LogInformation("PlaceDeletedEvent published for place {PlaceId}", place.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PlaceDeletedEvent for place {PlaceId}. Place deletion was successful but search index may not be updated.", place.Id);
            }

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

        public async Task<ApiResponse<PagedResult<GetPlaceDto>>> SearchPlacesByLocationAsync(double latitude, double longitude, int radiusInMeters, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching places near latitude {Latitude}, longitude {Longitude} with radius {Radius} meters - page {Page}, pageSize {PageSize}", 
                latitude, longitude, radiusInMeters, request.Page, request.PageSize);

            if (!GeoLocationHelper.IsValidCoordinate(latitude, longitude))
            {
                _logger.LogWarning("Invalid search coordinates: latitude {Latitude}, longitude {Longitude}", latitude, longitude);
                return ApiResponse<PagedResult<GetPlaceDto>>.Failure(ErrorCode.ValidationError);
            }

            var pagedPlaces = await _placeRepository.GetTable()
                .Select(p => PlaceMapper.ToGetDto(p))
                .ToPagedResultAsync(request, cancellationToken);

            var nearbyPlaces = GeoLocationHelper.FilterByRadius(
                pagedPlaces.Items!,
                latitude,
                longitude,
                radiusInMeters,
                p => p.Latitude,
                p => p.Longitude).ToList();

            _logger.LogInformation("Found {PlaceCount} places within {Radius} meters out of {TotalCount}", pagedPlaces.Items.Count(), radiusInMeters, pagedPlaces.TotalCount);

            return ApiResponse<PagedResult<GetPlaceDto>>.Success(pagedPlaces);
        }
    }
}
