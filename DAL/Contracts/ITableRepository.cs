using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DAL.Contracts
{
    public interface ITableRepository<T> where T : BaseTable
    {
        Task<T> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);

        Task AddAsync(T entity, Guid userId);
        Task UpdateAsync(T entity, Guid userId);
        Task DeleteAsync(Guid id);
        Task ChangeStatusAsync(
            Guid id,
            Guid userId,
            RecordStatus recordStatus = RecordStatus.Deleted);

        IQueryable<T> GetTable();

        Task ExecuteInTransactionAsync(
    Func<Task> action,
    System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);
    }
}
