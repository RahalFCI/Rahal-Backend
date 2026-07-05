using Shared.Application.Pagination;
using SocialMedia.Application.DTOs.Users;

namespace SocialMedia.Application.Interfaces
{
    public interface IUserGateway
    {
        Task<Dictionary<Guid, string>> GetUserDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, UserGatewayUserDto>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
        Task<UserGatewayPagedResult> GetUsersPaginatedAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
    }
}
