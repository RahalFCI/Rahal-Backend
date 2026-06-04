using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application.Interfaces;
using Shared.Domain.Entities;
using System.Linq.Expressions;

namespace Shared.Infrastructure.Repositories
{
    public abstract class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
        where TContext : DbContext
    {
        protected readonly TContext Context;

        protected GenericRepository(TContext context)
        {
            Context = context;
        }

        public async Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>().FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<TEntity?>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>().ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity?>> GetAllByExpression(Expression<Func<TEntity, bool>> conditionExpression, CancellationToken cancellationToken)
        {
            return await Context.Set<TEntity>().Where(conditionExpression).ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> GetByExpression(Expression<Func<TEntity, bool>> conditionExpression, CancellationToken cancellationToken)
        {
            return await Context.Set<TEntity>().FirstOrDefaultAsync(conditionExpression, cancellationToken);
        }

        public IQueryable<TEntity> GetTable()
        {
            return Context.Set<TEntity>();
        }

        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public void Update(TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }

        public void SaveInclude(TEntity entity, params string[] includedProperties)
        {
            var localEntity = Context.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entity.Id);

            EntityEntry entry;
            if (localEntity == null)
            {
                Context.Set<TEntity>().Attach(entity);
                entry = Context.Entry(entity);
            }
            else
            {
                entry = Context.Entry(localEntity);
                Context.Entry(localEntity).CurrentValues.SetValues(entity);
            }
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;
                property.IsModified = includedProperties.Contains(property.Metadata.Name);
            }
        }
    }
}
