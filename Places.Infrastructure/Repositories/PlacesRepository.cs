using Places.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;
using Places.Application.Interfaces;

namespace Places.Infrastructure.Repositories
{
    public class PlacesRepository<TEntity> : GenericRepository<TEntity, PlacesDbContext>, IPlacesRepository<TEntity>
        where TEntity : BaseEntity
    {
        public PlacesRepository(PlacesDbContext context)
            : base(context)
        {
        }
    }
}
