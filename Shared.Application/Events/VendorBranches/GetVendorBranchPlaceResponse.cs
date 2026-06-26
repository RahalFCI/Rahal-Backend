using Shared.Domain.Enums;

namespace Shared.Application.Events.VendorBranches
{
    public record GetVendorBranchPlaceResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        VendorBranchPlaceDto? Place);
}
