using Shared.Application.DTOs;
using SocialMedia.Application.DTOs.Posts;

namespace SocialMedia.Application.Interfaces
{
    public interface IPostService
    {
        Task<ApiResponse<PostResponse>> CreatePostAsync(
            CreatePostRequest request,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<string>> LikePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
