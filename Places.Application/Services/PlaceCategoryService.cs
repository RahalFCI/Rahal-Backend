using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Places.Application.DTOs.PlaceCategory;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

            var category = await _categoryRepository.GetTable().Include(c => c.Places).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

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

            var categories = await _categoryRepository.GetTable().Include(c => c.Places).ToListAsync(cancellationToken);
            var dtos = PlaceCategoryMapper.ToGetDtos(categories);

            _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count());

            return ApiResponse<IEnumerable<GetPlaceCategoryDto>>.Success(dtos);
        }

        public async Task<ApiResponse<string>> CreatePlaceCategoryAsync(CreatePlaceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new category");

            var category = PlaceCategoryMapper.ToEntity(dto);
            _categoryRepository.Add(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);

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
            await _categoryRepository.SaveChangesAsync(cancellationToken);

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

            var placesInCategory = await _placeRepository.GetTable().Where(p => p.PlaceCategoryId == id).ToListAsync(cancellationToken);
            if (placesInCategory.Any())
            {
                _logger.LogWarning("Cannot delete category {CategoryId} with existing places", id);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            category.IsDeleted = true;
            await _categoryRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} deleted successfully", id);

            return ApiResponse<string>.Success("Category deleted successfully");
        }

        public async Task<ApiResponse<string>> DeletePlaceCategoryPermanentlyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Permanently deleting category {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found for permanent deletion", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var placesInCategory = await _placeRepository.GetTable()
                .IgnoreQueryFilters()
                .Where(p => p.PlaceCategoryId == id)
                .ToListAsync(cancellationToken);

            if (placesInCategory.Any())
            {
                _logger.LogWarning("Cannot permanently delete category {CategoryId} with associated places", id);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} permanently deleted", id);

            return ApiResponse<string>.Success("Category permanently deleted");
        }
    }
}
