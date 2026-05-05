using Shared.Domain.Events;

namespace Shared.Domain.Events
{
    public record VendorProfileCreatedEvent(
        Guid UserId,
        string CountryCode,
        string Address,
        string AddressUrl,
        Dictionary<DayOfWeek, string> WorkingHours,
        Guid CategoryId) : BaseDomainEvent;
}
