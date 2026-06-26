namespace Gamification.Application.DTOs.VendorBranches
{
    public class VendorBranchDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public Guid PlaceId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }
}
