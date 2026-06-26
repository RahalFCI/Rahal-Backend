using Gamification.Application.DTOs.VendorBranches;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Queries.VendorBranches
{
    public record GetVendorBranchByIdQuery(Guid VendorId, Guid BranchId)
        : IRequest<ApiResponse<VendorBranchDto>>;
}
