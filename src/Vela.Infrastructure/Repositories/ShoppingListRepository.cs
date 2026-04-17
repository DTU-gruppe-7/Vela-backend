using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities.ShoppingList;
using Vela.Domain.Entities.Recipes;
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
            .ThenInclude(i => i.MealPlanEntry)
            .ThenInclude(mpe => mpe.Recipe)
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
            .ThenInclude(i => i.MealPlanEntry)
            .ThenInclude(mpe => mpe.Recipe)
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
            .ThenInclude(i => i.MealPlanEntry)
            .ThenInclude(mpe => mpe.Recipe)
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
            .Include(sl => sl.MealPlanEntry)
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

    public async Task DeleteItemsByMealPlanEntryIdAsync(Guid mealPlanEntryId)
    {
        var items = await _context.Set<ShoppingListItem>()
            .Where(sl => sl.MealPlanEntryId == mealPlanEntryId)
            .ToListAsync();

        if (items.Count > 0)
            _context.Set<ShoppingListItem>().RemoveRange(items);
    }

    public async Task RemoveRangeAsync(IEnumerable<ShoppingListItem> items)
    {
        _context.Set<ShoppingListItem>().RemoveRange(items);
    }

    public async Task<List<ShoppingListItem>> GetItemsAssignedToUserAsync(string userId)
    {
        return await _context.Set<ShoppingListItem>()
            .Include(i => i.MealPlanEntry)
            .ThenInclude(mpe => mpe.Recipe)
            .Where(i => i.AssignedUserId == userId && !i.IsBought) // Vi henter kun ting der ikke er købt
            .AsNoTracking() 
            .ToListAsync();
    }

}