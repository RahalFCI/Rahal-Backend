namespace Gamification.Application.DTOs.VendorBranches
{
    public class CreateVendorBranchAddressDto
    {
        public string AddressLine { get; set; } = string.Empty;
        public string Government { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
