namespace Gamification.Application.DTOs.VendorBranches
{
    public class UpdateVendorBranchDto
    {
        public string BranchName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string PlaceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeoFenceRange { get; set; } = 50;
        public UpdateVendorBranchAddressDto? Address { get; set; }
    }
}
