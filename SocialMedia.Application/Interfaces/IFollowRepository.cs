using Shared.Application.Pagination;
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
        Task<List<Guid>> GetFolloweeIdsByFollowerAsync(Guid followerId, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetFollowerIdsByFolloweeAsync(Guid followeeId, CancellationToken cancellationToken = default);
        Task<PagedResult<Guid>> GetFollowerIdsByFolloweePaginatedAsync(Guid followeeId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<Guid>> GetFolloweeIdsByFollowerPaginatedAsync(Guid followerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> CountFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> CountFollowersByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> CountFollowingByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
        void Add(Follow follow);
        void Remove(Follow follow);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
