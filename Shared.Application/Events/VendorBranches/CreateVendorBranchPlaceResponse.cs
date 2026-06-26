using Shared.Domain.Enums;

namespace Shared.Application.Events.VendorBranches
{
    public record CreateVendorBranchPlaceResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        VendorBranchPlaceDto? Place);
}
