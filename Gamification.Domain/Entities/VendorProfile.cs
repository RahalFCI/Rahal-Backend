using Shared.Domain.Entities;

namespace Gamification.Domain.Entities
{
    public class VendorProfile : BaseEntity
    {
        public required Guid UserId { get; set; }

        public required string CountryCode { get; set; }

        public required string Address { get; set; }

        public required string AddressUrl { get; set; }

        public required Dictionary<DayOfWeek, string> WorkingHours { get; set; }

        public required Guid CategoryId { get; set; }

        public VendorCategory? Category { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
