using Shared.Domain.Enums;

namespace Shared.Application.Events.VendorBranches
{
    public record GetVendorBranchPlacesResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        IEnumerable<VendorBranchPlaceDto> Places);
}
