namespace Shared.Application.Events.VendorBranches
{
    public record UpdateVendorBranchPlaceRequest(
        Guid OperationId,
        Guid PlaceId,
        string Name,
        string Description,
        double Latitude,
        double Longitude,
        int GeoFenceRange,
        VendorBranchPlaceAddressDto? Address);
}
