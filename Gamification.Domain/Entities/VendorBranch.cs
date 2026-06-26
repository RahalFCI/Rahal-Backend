using Shared.Domain.Entities;

namespace Gamification.Domain.Entities
{
    public class VendorBranch : BaseEntity
    {
        public Guid VendorId { get; set; }
        public Guid PlaceId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
