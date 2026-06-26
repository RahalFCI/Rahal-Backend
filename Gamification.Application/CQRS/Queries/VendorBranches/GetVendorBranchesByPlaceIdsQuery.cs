using Gamification.Application.DTOs.VendorBranches;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Gamification.Application.CQRS.Queries.VendorBranches
{
    public record GetVendorBranchesByPlaceIdsQuery(IEnumerable<Guid> PlaceIds, OffsetPaginationRequest PaginationRequest)
        : IRequest<ApiResponse<PagedResult<VendorBranchDto>>>;
}
