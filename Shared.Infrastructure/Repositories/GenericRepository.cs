using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application.Interfaces;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Shared.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly DbContext _dbContext;

        public GenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<T>().FindAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<T?>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<T>().ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T?>> GetAllByExpression(Expression<Func<T, bool>> conditionExpression, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<T>().Where(conditionExpression).ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByExpression(Expression<Func<T, bool>> conditionExpression, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(conditionExpression, cancellationToken);
        }

        public IQueryable<T> GetTable()
        {
            return _dbContext.Set<T>();
        }

        public void Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void SaveInclude(T entity, params string[] includedProperties)
        {
            var localEntity = _dbContext.Set<T>().Local.FirstOrDefault(e => e.Id == entity.Id);

            EntityEntry entry;
            if (localEntity == null)
            {
                _dbContext.Set<T>().Attach(entity);
                entry = _dbContext.Entry(entity);
            }
            else
            {
                entry = _dbContext.Entry(localEntity);
                _dbContext.Entry(localEntity).CurrentValues.SetValues(entity);
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
