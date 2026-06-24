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

        Task<ApiResponse<string>> UnlikePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>> CreateCommentAsync(
            Guid postId,
            Guid userId,
            SocialMedia.Application.DTOs.Comments.CreateCommentRequest request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<string>> DeleteCommentAsync(
            Guid commentId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
