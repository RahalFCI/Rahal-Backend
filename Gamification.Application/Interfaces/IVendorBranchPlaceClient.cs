using Gamification.Application.DTOs.VendorBranches;
using Shared.Application.DTOs;
using Shared.Application.Events.VendorBranches;

namespace Gamification.Application.Interfaces
{
    public interface IVendorBranchPlaceClient
    {
        Task<ApiResponse<VendorBranchPlaceDto>> CreatePlaceAsync(CreateVendorBranchDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<VendorBranchPlaceDto>> UpdatePlaceAsync(Guid placeId, UpdateVendorBranchDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<VendorBranchPlaceDto>> GetPlaceAsync(Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<VendorBranchPlaceDto>>> GetPlacesAsync(IEnumerable<Guid> placeIds, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlaceAsync(Guid placeId, CancellationToken cancellationToken = default);
    }
}
