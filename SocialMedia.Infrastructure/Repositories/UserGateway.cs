using Shared.Application.Pagination;
using SocialMedia.Application.DTOs.Users;
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

        public async Task<Dictionary<Guid, UserGatewayUserDto>> GetUsersByIdsAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            var users = await _usersPublicApi.GetUsersDisplayNamesAsync(userIds, cancellationToken);
            return users.ToDictionary(
                u => u.Id,
                u => new UserGatewayUserDto
                {
                    Id = u.Id,
                    Name = u.DisplayName
                });
        }

        public async Task<UserGatewayPagedResult> GetUsersPaginatedAsync(
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            var users = await _usersPublicApi.GetUsersPaginatedAsync(
                request.Page,
                request.PageSize,
                cancellationToken);

            return new UserGatewayPagedResult
            {
                Items = users.Items.Select(u => new UserGatewayUserDto
                {
                    Id = u.Id,
                    Name = u.DisplayName
                }),
                TotalCount = users.TotalCount,
                Page = users.Page,
                PageSize = users.PageSize
            };
        }
    }
}
