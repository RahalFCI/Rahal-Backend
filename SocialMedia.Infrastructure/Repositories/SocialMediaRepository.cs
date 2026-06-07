using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;
using SocialMedia.Infrastructure.Persistence;


namespace SocialMedia.Infrastructure.Repositories
{
    public class SocialMediaRepository<TEntity> : GenericRepository<TEntity, SocialMediaDbContext>
        where TEntity : BaseEntity
    {
        public SocialMediaRepository(SocialMediaDbContext context)
            : base(context)
        {
        }
    }
}
