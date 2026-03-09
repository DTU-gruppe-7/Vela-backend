using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class ShoppingListRepository : Repository<ShoppingList>, IShoppingListRepository
{
    public ShoppingListRepository(AppDbContext context) : base(context)
    {
        
    }
    
    /// <summary>
    /// Get shopping list with all items and ingredients
    /// Uses eager loading to avoid N+1 query problem
    /// </summary>
    public async Task<ShoppingList?> GetByIdWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(sl => sl.Items)!
            .ThenInclude(i => i.Ingredient)
            .AsNoTracking()
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    /// <summary>
    /// Get all shopping lists for a user with items
    /// Uses eager loading to avoid N+1 query problem
    /// </summary>
    public async Task<IEnumerable<ShoppingList>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(sl => sl.Items)! 
            .ThenInclude(i => i.Ingredient)
            .Where(sl => sl.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Get all shopping lists for a group with items
    /// Uses eager loading to avoid N+1 query problem
    /// </summary>
    public async Task<IEnumerable<ShoppingList>> GetByGroupId(Guid groupId)
    {
        return await _dbSet
            .Include(sl => sl.Items)! 
            .ThenInclude(i => i.Ingredient)
            .Where(sl => sl.GroupId == groupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ShoppingListItem?> GetItemByIdAsync(Guid id)
    {
        return await _context.Set<ShoppingListItem>()
            .Include(i => i.Ingredient)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    public async Task<ShoppingListItem?> AddItemAsync(ShoppingListItem item)
    {
        await _context.Set<ShoppingListItem>().AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }
}