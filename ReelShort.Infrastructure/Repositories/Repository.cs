using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ReelShort.Application.Interfaces;
using ReelShort.Infrastructure.Data;

namespace ReelShort.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    #region Main method

    public async Task<T?> GetByIdAsync(Guid id, bool isTracking = false)
    {
        return await GetQuery(isTracking).FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    public async Task<List<T>> GetAllAsync(bool isTracking = false)
    {
        return await GetQuery(isTracking).ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool isTracking = false)
    {
        return await GetQuery(isTracking).Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool isTracking = false)
    {
        return await GetQuery(isTracking).FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    #endregion Main method
    
    #region Private method
    
    private IQueryable<T> GetQuery(bool isTracking = false)
    {
        return isTracking ? _dbSet : _dbSet.AsNoTracking();
    }
    
    #endregion Private method
}