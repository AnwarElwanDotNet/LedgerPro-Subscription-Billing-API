using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DAL.Contracts
{
    public interface IViewRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);
        IQueryable<T> GetTable();
    }
}
