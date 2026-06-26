namespace Gamification.Application.DTOs.VendorBranches
{
    public class CreateVendorBranchDto
    {
        public string BranchName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeoFenceRange { get; set; } = 50;
        public VendorBranchAddressDto? Address { get; set; }
    }
}
