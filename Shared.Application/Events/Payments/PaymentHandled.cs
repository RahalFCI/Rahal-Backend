using Shared.Domain.Events;

namespace Shared.Application.Events.Payments
{
    public record PaymentHandled(
        Guid PaymentId,
        Guid OperationId,
        Guid ExplorerId,
        Guid ReferenceId,
        decimal Amount,
        string Currency,
        string Status,
        string Gateway,
        string? GatewayPaymentIntentId,
        DateTime HandledAt) : BaseDomainEvent;
}
