namespace Shared.Application.Interfaces
{
    public interface IFcmNotificationService
    {
        Task SendMulticastAsync(
            IReadOnlyList<string> deviceTokens,
            string title,
            string body,
            Dictionary<string, string> dataPayload);
    }
}
