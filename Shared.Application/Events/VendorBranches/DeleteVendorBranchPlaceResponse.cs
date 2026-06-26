using Shared.Domain.Enums;

namespace Shared.Application.Events.VendorBranches
{
    public record DeleteVendorBranchPlaceResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        string? Message);
}
