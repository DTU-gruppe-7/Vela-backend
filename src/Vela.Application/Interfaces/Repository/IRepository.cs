namespace Vela.Application.Interfaces.Repository;

public interface IRepository<T>
{
    Task AddAsync(T entity);
    Task<T?> GetByUuidAsync(Guid uuid);
    Task<IEnumerable<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}