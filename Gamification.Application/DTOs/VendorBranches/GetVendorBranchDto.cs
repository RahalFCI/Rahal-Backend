namespace Gamification.Application.DTOs.VendorBranches
{
    public class GetVendorBranchDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public Guid PlaceId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeoFenceRange { get; set; }
        public GetVendorBranchAddressDto? Address { get; set; }
    }
}
