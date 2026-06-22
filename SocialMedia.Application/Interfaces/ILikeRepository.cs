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
        void Add(Like like);
        void Remove(Like like);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
