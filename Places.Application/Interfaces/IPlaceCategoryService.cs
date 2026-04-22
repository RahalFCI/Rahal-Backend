using Shared.Application.DTOs;
using Places.Application.DTOs.PlaceCategory;

namespace Places.Application.Interfaces
{
    public interface IPlaceCategoryService
    {
        Task<ApiResponse<GetPlaceCategoryDto>> GetPlaceCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceCategoryDto>>> GetAllPlaceCategoriesAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> CreatePlaceCategoryAsync(CreatePlaceCategoryDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePlaceCategoryAsync(Guid id, UpdatePlaceCategoryDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlaceCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
