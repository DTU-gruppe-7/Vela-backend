using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<T?> GetByUuidAsync(Guid uuid)
    {
        return await _dbSet.FindAsync(uuid);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid uuid)
    {
        _dbSet.Remove(_dbSet.Find(uuid));
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid uuid)
    {
        return await _dbSet.FindAsync(uuid) != null;
    }
}