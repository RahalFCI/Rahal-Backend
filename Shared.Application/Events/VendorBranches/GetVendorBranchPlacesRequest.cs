namespace Shared.Application.Events.VendorBranches
{
    public record GetVendorBranchPlacesRequest(Guid OperationId, IEnumerable<Guid> PlaceIds);
}
