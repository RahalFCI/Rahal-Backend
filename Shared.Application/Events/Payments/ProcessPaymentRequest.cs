namespace Shared.Application.Events.Payments
{
    public record ProcessPaymentRequest(
        Guid OperationId,
        Guid ExplorerId,
        decimal Amount,
        string Currency,
        string PaymentMethod,
        Guid ReferenceId);
}
