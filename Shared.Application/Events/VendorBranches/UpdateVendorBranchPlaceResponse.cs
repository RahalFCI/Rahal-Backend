using Shared.Domain.Enums;

namespace Shared.Application.Events.VendorBranches
{
    public record UpdateVendorBranchPlaceResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        VendorBranchPlaceDto? Place);
}
