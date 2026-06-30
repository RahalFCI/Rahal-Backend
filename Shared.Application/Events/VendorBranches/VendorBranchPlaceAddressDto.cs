namespace Shared.Application.Events.VendorBranches
{
    public record VendorBranchPlaceAddressDto(
        string AddressLine,
        string Government,
        string City,
        string Country);
}
