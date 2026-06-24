namespace Shared.Application.Events.Payments
{
    public record SetExplorerPremiumRequest(
        Guid OperationId,
        Guid ExplorerId,
        bool IsPremium);
}
