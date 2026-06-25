using Shared.Application.DTOs;
using SocialMedia.Application.DTOs.Follows;

namespace SocialMedia.Application.Interfaces
{
    public interface IFollowService
    {
        Task<ApiResponse<FollowResponse>> FollowAsync(
            Guid followerId,
            Guid followingId,
            CancellationToken cancellationToken = default);
    }
}
