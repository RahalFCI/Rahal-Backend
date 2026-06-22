using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Repository for the Follow junction table (composite PK: FollowerId + FolloweeId).
    /// Cannot use the generic repository because Follow does not inherit BaseEntity.
    /// </summary>
    public interface IFollowRepository
    {
        Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
        void Add(Follow follow);
        void Remove(Follow follow);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
