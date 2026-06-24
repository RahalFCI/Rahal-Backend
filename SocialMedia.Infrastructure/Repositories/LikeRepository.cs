using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Persistence;

namespace SocialMedia.Infrastructure.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly SocialMediaDbContext _context;

        public LikeRepository(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<Like?> GetAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
        {
            return await _context.Likes
                .FindAsync(new object[] { userId, postId }, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
        {
            return await _context.Likes
                .AnyAsync(l => l.UserId == userId && l.PostId == postId, cancellationToken);
        }

        /// <summary>
        /// Returns all PostIds liked by a user. Used to hydrate the UserLikes:{UserId} Redis Set on cache miss.
        /// </summary>
        public async Task<List<Guid>> GetPostIdsLikedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Likes
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .Select(l => l.PostId)
                .ToListAsync(cancellationToken);
        }

        public void Add(Like like) => _context.Likes.Add(like);

        public void Remove(Like like) => _context.Likes.Remove(like);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
