using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;
using SocialMedia.Application.Interfaces;
using SocialMedia.Infrastructure.Persistence;

namespace SocialMedia.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository for SocialMedia entities that inherit BaseEntity (Post, Comment).
    /// </summary>
    public class SocialMediaRepository<TEntity> : GenericRepository<TEntity, SocialMediaDbContext>, ISocialMediaRepository<TEntity>
        where TEntity : BaseEntity
    {
        public SocialMediaRepository(SocialMediaDbContext context)
            : base(context)
        {
        }
    }
}
