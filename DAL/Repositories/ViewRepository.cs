using DAL.Contracts;
using DAL.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DAL.Repositories
{
    public class ViewRepository<T> : IViewRepository<T> where T : class
    {
        private readonly LedgerProDBContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly bool _hasCurrentState;

        public ViewRepository(LedgerProDBContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _hasCurrentState = typeof(T).GetProperty("CurrentState") != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (_hasCurrentState)
            {
                query = query.Where(x => EF.Property<int>(x, "CurrentState") > 0);
            }

            return await query.ToListAsync();
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(filter);

            if (_hasCurrentState)
            {
                query = query.Where(x => EF.Property<int>(x, "CurrentState") > 0);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(filter);

            if (_hasCurrentState)
            {
                query = query.Where(x => EF.Property<int>(x, "CurrentState") > 0);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            query = query.Where(x => EF.Property<Guid>(x, "Id") == id);

            if (_hasCurrentState)
            {
                query = query.Where(x => EF.Property<int>(x, "CurrentState") > 0);
            }

            return await query.FirstOrDefaultAsync();
        }

        public Task<List<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes)
        {
            throw new NotSupportedException("Views do not support Includes.");
        }

        public IQueryable<T> GetTable()
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (_hasCurrentState)
            {
                query = query.Where(x => EF.Property<int>(x, "CurrentState") > 0);
            }

            return query;
        }
    }
}
