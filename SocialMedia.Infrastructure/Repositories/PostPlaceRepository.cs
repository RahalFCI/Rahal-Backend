using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Persistence;

namespace SocialMedia.Infrastructure.Repositories
{
    public class PostPlaceRepository : IPostPlaceRepository
    {
        private readonly SocialMediaDbContext _context;

        public PostPlaceRepository(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<PostPlace?> GetAsync(Guid postId, Guid placeId, CancellationToken cancellationToken = default)
        {
            return await _context.PostPlaces
                .FindAsync(new object[] { postId, placeId }, cancellationToken);
        }

        public async Task<IEnumerable<PostPlace>> GetByPostAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            return await _context.PostPlaces
                .Where(pp => pp.PostId == postId)
                .ToListAsync(cancellationToken);
        }

        public void Add(PostPlace postPlace) => _context.PostPlaces.Add(postPlace);

        public void Remove(PostPlace postPlace) => _context.PostPlaces.Remove(postPlace);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
