using Gamification.Application.DTOs.VendorBranches;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.VendorBranches
{
    public record CreateVendorBranchCommand(Guid VendorId, Guid PlaceId, CreateVendorBranchDto Dto)
        : IRequest<ApiResponse<VendorBranchDto>>;
}
