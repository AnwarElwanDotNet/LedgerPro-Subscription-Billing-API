using DAL.Contracts;
using DAL.DBContext;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DAL.Repositories
{
    public class TableRepository<T> : ITableRepository<T> where T : BaseTable
    {
        private readonly LedgerProDBContext _context;
        private readonly DbSet<T> _dbSet;

        public TableRepository(LedgerProDBContext LedgerProContext)
        {
            _context = LedgerProContext;
            _dbSet = LedgerProContext.Set<T>();
        }

        public async Task AddAsync(T entity, Guid userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreatedDate = DateTime.UtcNow;
            entity.CurrentState = 1;
            entity.CreatedBy = userId;
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(Guid id, Guid userId, RecordStatus recordStatus = RecordStatus.Deleted)
        {
            var entity = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new Exception("Entity not found");

            entity.CurrentState = (int)recordStatus;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new Exception("Entity not found");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.CurrentState > 0)
                .ToListAsync();
        }

        public async Task<List<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet
                .AsNoTracking()
                .Where(x => x.CurrentState > 0);

            foreach (var include in includes)
                query = query.Include(include);

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.CurrentState > 0);
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.CurrentState > 0)
                .Where(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(filter)
                .Where(x => x.CurrentState > 0)
                .ToListAsync();
        }

        public async Task UpdateAsync(T entity, Guid userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var data = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (data == null)
                throw new Exception("Entity not found");

            if (data.CurrentState == (int)RecordStatus.Deleted)
            {
                throw new Exception("Entity Already Deleted");
            }

            entity.CurrentState = data.CurrentState;
            entity.CreatedDate = data.CreatedDate;
            entity.CreatedBy = data.CreatedBy;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            _context.Entry(data).CurrentValues.SetValues(entity);

            await _context.SaveChangesAsync();
        }
        public IQueryable<T> GetTable()
        {
            return _dbSet
                .AsNoTracking()
                .Where(x => x.CurrentState > 0);
        }

        public async Task ExecuteInTransactionAsync(
        Func<Task> action,
        System.Data.IsolationLevel isolationLevel =
            System.Data.IsolationLevel.ReadCommitted)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(isolationLevel);

            try
            {
                await action();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
