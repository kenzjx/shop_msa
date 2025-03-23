using System.Linq.Expressions;

namespace Application.Commom.Interfaces;

public interface IRepositoryMongo<T> where T : class
{
    Task<T> GetByIdAsync(string id);
    
    Task<IEnumerable<T>> GetAllAsync();
    
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    Task AddAsync(T entity);
    
    Task UpdateAsync(string id, T entity);
    
    Task DeleteAsync(string id);
}