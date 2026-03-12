using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken);
        Task<IEnumerable<T?>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<T?>> GetAllByExpression(Expression<Func<T, bool>> conditionExpression, CancellationToken cancellationToken);
        Task<T?> GetByExpression(Expression<Func<T, bool>> conditionExpression, CancellationToken cancellationToken);
        IQueryable<T> GetTable();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
