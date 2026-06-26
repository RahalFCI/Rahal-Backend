using Gamification.Application.DTOs.VendorBranches;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.VendorBranches
{
    public record UpdateVendorBranchCommand(Guid BranchId, UpdateVendorBranchDto Dto)
        : IRequest<ApiResponse<GetVendorBranchDto>>;
}
