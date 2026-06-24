namespace Shared.Application.Events.Payments
{
    public record SpendXpRequest(
        Guid OperationId,
        Guid ExplorerId,
        int Amount,
        string SourceType,
        Guid ReferenceId);
}
