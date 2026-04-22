using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Places.Application.DTOs.PlaceCategory;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;

namespace Places.Application.Services
{
    internal class PlaceCategoryService : IPlaceCategoryService
    {
        private readonly IGenericRepository<PlaceCategory> _categoryRepository;
        private readonly IGenericRepository<Place> _placeRepository;
        private readonly ILogger<PlaceCategoryService> _logger;

        public PlaceCategoryService(
            IGenericRepository<PlaceCategory> categoryRepository,
            IGenericRepository<Place> placeRepository,
            ILogger<PlaceCategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _placeRepository = placeRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetPlaceCategoryDto>> GetPlaceCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching category {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id, p => p.Places, cancellationToken);

            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return ApiResponse<GetPlaceCategoryDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetPlaceCategoryDto>.Success(PlaceCategoryMapper.ToGetDto(category));
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceCategoryDto>>> GetAllPlaceCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all categories");

            var categories = await _categoryRepository.GetAllAsync(c => c.Places, cancellationToken);
            var dtos = PlaceCategoryMapper.ToGetDtos(categories);

            _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count());

            return ApiResponse<IEnumerable<GetPlaceCategoryDto>>.Success(dtos);
        }

        public async Task<ApiResponse<string>> CreatePlaceCategoryAsync(CreatePlaceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new category");

            var category = PlaceCategoryMapper.ToEntity(dto);
            await _categoryRepository.AddAsync(category, cancellationToken);

            _logger.LogInformation("Category created successfully with ID {CategoryId}", category.Id);

            return ApiResponse<string>.Success($"Category created successfully. ID: {category.Id}");
        }

        public async Task<ApiResponse<string>> UpdatePlaceCategoryAsync(Guid id, UpdatePlaceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating category {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            PlaceCategoryMapper.UpdateEntity(category, dto);
            await _categoryRepository.UpdateAsync(category, cancellationToken);

            _logger.LogInformation("Category {CategoryId} updated successfully", id);

            return ApiResponse<string>.Success("Category updated successfully");
        }

        public async Task<ApiResponse<string>> DeletePlaceCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting category {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var placesInCategory = await _placeRepository.FindAsync(p => p.PlaceCategoryId == id, cancellationToken);
            if (placesInCategory.Any())
            {
                _logger.LogWarning("Cannot delete category {CategoryId} with existing places", id);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError, "Cannot delete category with existing places");
            }

            await _categoryRepository.DeleteAsync(category, cancellationToken);

            _logger.LogInformation("Category {CategoryId} deleted successfully", id);

            return ApiResponse<string>.Success("Category deleted successfully");
        }
    }
}
