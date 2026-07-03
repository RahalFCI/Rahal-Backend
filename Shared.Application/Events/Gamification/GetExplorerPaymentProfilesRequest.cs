namespace Shared.Application.Events.Gamification
{
    public record GetExplorerPaymentProfilesRequest(
        string? DisplayName,
        IReadOnlyCollection<Guid>? ExplorerIds);
}
