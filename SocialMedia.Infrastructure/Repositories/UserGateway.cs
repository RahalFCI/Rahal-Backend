using SocialMedia.Application.Interfaces;
using Users.Contracts.Interfaces;

namespace SocialMedia.Infrastructure.Repositories
{
    public class UserGateway : IUserGateway
    {
        private readonly IUsersPublicApi _usersPublicApi;

        public UserGateway(IUsersPublicApi usersPublicApi)
        {
            _usersPublicApi = usersPublicApi;
        }

        public async Task<Dictionary<Guid, string>> GetUserDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var users = await _usersPublicApi.GetUsersDisplayNamesAsync(userIds, cancellationToken);
            return users.ToDictionary(u => u.Id, u => u.DisplayName);
        }
    }
}
