using Places.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;

namespace Places.Infrastructure.Repositories
{
    public class PlacesRepository<TEntity> : GenericRepository<TEntity, PlacesDbContext>
        where TEntity : BaseEntity
    {
        public PlacesRepository(PlacesDbContext context)
            : base(context)
        {
        }
    }
}
