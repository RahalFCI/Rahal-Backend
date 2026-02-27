using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync<TKey>(TKey id);
        Task<IEnumerable<T?>> GetAllAsync();
        Task<IEnumerable<T?>> GetAllByExpression(Expression<Func<T, bool>> conditionExpression);
        Task<T?> GetProductByExpression(Expression<Func<T, bool>> conditionExpression);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
