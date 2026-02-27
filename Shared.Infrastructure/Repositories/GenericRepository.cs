using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Shared.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbContext _dbContext;

        public GenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T?> GetByIdAsync<TKey>(TKey id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T?>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T?>> GetAllByExpression(Expression<Func<T, bool>> conditionExpression)
        {
            return await _dbContext.Set<T>().Where(conditionExpression).ToListAsync();
        }

        public async Task<T?> GetProductByExpression(Expression<Func<T, bool>> conditionExpression)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(conditionExpression);
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
    }
}
