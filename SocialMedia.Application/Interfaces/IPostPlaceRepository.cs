using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Repository for the PostPlace junction table (composite PK: PostId + PlaceId).
    /// Cannot use the generic repository because PostPlace does not inherit BaseEntity.
    /// </summary>
    public interface IPostPlaceRepository
    {
        Task<PostPlace?> GetAsync(Guid postId, Guid placeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PostPlace>> GetByPostAsync(Guid postId, CancellationToken cancellationToken = default);
        void Add(PostPlace postPlace);
        void Remove(PostPlace postPlace);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
