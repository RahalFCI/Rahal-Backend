namespace SocialMedia.Application.Interfaces
{
    public interface IUserGateway
    {
        Task<Dictionary<Guid, string>> GetUserDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
