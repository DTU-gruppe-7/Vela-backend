using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class ShoppingListRepository(AppDbContext context) : Repository<ShoppingList>(context), IShoppingListRepository
{
    
    /// <summary>
    /// Get shopping list with all items and ingredients
    /// Uses eager loading to avoid N+1 query problem
    /// </summary>
    public async Task<ShoppingList?> GetByIdWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(sl => sl.Items)!
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    /// <summary>
    /// Get all shopping lists for a user with items
    /// Uses eager loading to avoid N+1 query problem
    /// </summary>
    public async Task<ShoppingList?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(sl => sl.Items)! 
            .AsNoTracking()
            .SingleOrDefaultAsync(sl => sl.UserId == userId);
    }

    /// <summary>
    /// Get all shopping lists for a group with items
    /// Uses eager loading to avoid N+1 query problem
    /// </summary>
    public async Task<ShoppingList?> GetByGroupIdAsync(Guid groupId)
    {
        return await _dbSet
            .Include(sl => sl.Items)! 
            .AsNoTracking()
            .SingleOrDefaultAsync(sl => sl.GroupId == groupId);
    }
    
    public async Task<ShoppingList?> GetByIdWithItemsReadOnlyAsync(Guid id)
    {
        return await _dbSet
            .Include(sl => sl.Items)!
            .AsNoTracking()
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    public async Task<ShoppingListItem?> GetItemByIdAsync(Guid id)
    {
        return await _context.Set<ShoppingListItem>()
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    public async Task<ShoppingListItem?> AddItemAsync(ShoppingListItem item)
    {
        await _context.Set<ShoppingListItem>().AddAsync(item);
        return item;
    }

    public async Task<ShoppingListItem?> UpdateItemAsync(ShoppingListItem item)
    {
        _context.Set<ShoppingListItem>().Update(item);
        return item;
    }

    public async Task<ShoppingListItem?> DeleteItemAsync(Guid itemId)
    {
        var item = await _context.Set<ShoppingListItem>().FindAsync(itemId);
        if (item == null)
            return null;

        _context.Set<ShoppingListItem>().Remove(item);
        return item;
    }
}