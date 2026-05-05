using Shared.Domain.Enums;
using Shared.Domain.Events;

namespace Shared.Domain.Events
{
    public record ExplorerProfileCreatedEvent(
        Guid UserId,
        GenderEnum Gender,
        DateOnly BirthDate,
        string Bio,
        string CountryCode) : BaseDomainEvent;
}
