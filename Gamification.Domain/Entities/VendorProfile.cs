using Shared.Domain.Entities;

namespace Gamification.Domain.Entities
{
    public class VendorProfile : BaseEntity
    {
        public string DisplayName { get; set; } = string.Empty;

        public string ProfilePictureURL { get; set; } = string.Empty;

        public required Guid UserId { get; set; }

        public string CountryCode { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string AddressUrl { get; set; } = string.Empty;

        public Dictionary<DayOfWeek, string> WorkingHours { get; set; } = new Dictionary<DayOfWeek, string>();

        public Guid CategoryId { get; set; }

        public VendorCategory? Category { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
