using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Repository for the Like junction table (composite PK: UserId + PostId).
    /// Cannot use the generic repository because Like does not inherit BaseEntity.
    /// </summary>
    public interface ILikeRepository
    {
        Task<Like?> GetAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all PostIds liked by a user. Used to hydrate the UserLikes:{UserId} Redis Set on cache miss.
        /// </summary>
        Task<List<Guid>> GetPostIdsLikedByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<List<Guid>> GetUserIdsWhoLikedPostAsync(Guid postId, CancellationToken cancellationToken = default);

        void Add(Like like);
        void Remove(Like like);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
