using Microsoft.EntityFrameworkCore;
using Shared.Application.Pagination;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Persistence;

namespace SocialMedia.Infrastructure.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly SocialMediaDbContext _context;

        public FollowRepository(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .FindAsync(new object[] { followerId, followeeId }, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, cancellationToken);
        }

        public async Task<List<Guid>> GetFolloweeIdsByFollowerAsync(Guid followerId, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Guid>> GetFollowerIdsByFolloweeAsync(Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == followeeId)
                .Select(f => f.FollowerId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<Guid>> GetFollowerIdsByFolloweePaginatedAsync(
            Guid followeeId,
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            request.Page = request.Page <= 0 ? 1 : request.Page;
            request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == followeeId);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .ThenBy(f => f.FollowerId)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => f.FollowerId)
                .ToListAsync(cancellationToken);

            return new PagedResult<Guid>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<Guid>> GetFolloweeIdsByFollowerPaginatedAsync(
            Guid followerId,
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            request.Page = request.Page <= 0 ? 1 : request.Page;
            request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .ThenBy(f => f.FolloweeId)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken);

            return new PagedResult<Guid>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AsNoTracking()
                .CountAsync(f => f.FolloweeId == userId, cancellationToken);
        }

        public async Task<int> CountFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AsNoTracking()
                .CountAsync(f => f.FollowerId == userId, cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> CountFollowersByUserIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            if (userIds.Count == 0)
            {
                return new Dictionary<Guid, int>();
            }

            return await _context.Follows
                .AsNoTracking()
                .Where(f => userIds.Contains(f.FolloweeId))
                .GroupBy(f => f.FolloweeId)
                .Select(group => new
                {
                    UserId = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> CountFollowingByUserIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            if (userIds.Count == 0)
            {
                return new Dictionary<Guid, int>();
            }

            return await _context.Follows
                .AsNoTracking()
                .Where(f => userIds.Contains(f.FollowerId))
                .GroupBy(f => f.FollowerId)
                .Select(group => new
                {
                    UserId = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
        }

        public void Add(Follow follow) => _context.Follows.Add(follow);

        public void Remove(Follow follow) => _context.Follows.Remove(follow);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
