using Shared.Application.Interfaces;
using Shared.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Marker interface for the SocialMedia module's generic repository.
    /// Used for entities that inherit BaseEntity (Post, Comment).
    /// </summary>
    public interface ISocialMediaRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
    {
    }
}
