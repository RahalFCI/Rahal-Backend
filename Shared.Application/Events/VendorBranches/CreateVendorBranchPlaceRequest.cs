namespace Shared.Application.Events.VendorBranches
{
    public record CreateVendorBranchPlaceRequest(
        Guid OperationId,
        string Name,
        string Description,
        double Latitude,
        double Longitude,
        int GeoFenceRange,
        VendorBranchPlaceAddressDto? Address);
}
