using System.Linq.Expressions;

namespace ReelShort.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, bool isTracking = false);
    Task<List<T>> GetAllAsync(bool isTracking = false);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool isTracking = false);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool isTracking = false);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
}