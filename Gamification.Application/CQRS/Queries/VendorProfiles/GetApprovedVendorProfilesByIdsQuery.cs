using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Gamification.Application.CQRS.Queries.VendorProfiles
{
    public record GetApprovedVendorProfilesByIdsQuery(IEnumerable<Guid> VendorIds, OffsetPaginationRequest PaginationRequest)
        : IRequest<ApiResponse<PagedResult<GetVendorDto>>>;
}
