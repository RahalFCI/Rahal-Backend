using Shared.Application.DTOs;
using Shared.Application.Pagination;
using SocialMedia.Application.DTOs.Follows;
using SocialMedia.Application.DTOs.Users;

namespace SocialMedia.Application.Interfaces
{
    public interface IFollowService
    {
        Task<ApiResponse<FollowResponse>> FollowAsync(
            Guid followerId,
            Guid followingId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<FollowResponse>> UnfollowAsync(
            Guid followerId,
            Guid followingId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<PagedResult<SocialUserResponseDto>>> GetFollowersAsync(
            Guid userId,
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<PagedResult<SocialUserResponseDto>>> GetFolloweesAsync(
            Guid userId,
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<PagedResult<SocialUserResponseDto>>> GetSocialUsersAsync(
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default);
    }
}
