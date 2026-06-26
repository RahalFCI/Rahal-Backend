using Gamification.Application.DTOs.VendorBranches;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.VendorBranches
{
    public record DeleteVendorBranchCommand(Guid VendorId, Guid BranchId)
        : IRequest<ApiResponse<VendorBranchDto>>;
}
