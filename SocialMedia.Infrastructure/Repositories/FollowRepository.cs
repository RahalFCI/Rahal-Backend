using Microsoft.EntityFrameworkCore;
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

        public void Add(Follow follow) => _context.Follows.Add(follow);

        public void Remove(Follow follow) => _context.Follows.Remove(follow);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
