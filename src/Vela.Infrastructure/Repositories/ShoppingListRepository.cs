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
    
    public async Task<ShoppingList?> GetByIdWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(sl => sl.Items)!
                .ThenInclude(i => i.Ingredient)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    public async Task<IEnumerable<ShoppingList>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            //.Include(sl => sl.Items)
            .Where(sl => sl.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShoppingList>> GetByGroupId(Guid groupId)
    {
        return await  _dbSet
            //.Include(sl => sl.Items)
            .Where(sl => sl.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<ShoppingListItem?> GetItemByIdAsync(Guid id)
    {
        return await _context.Set<ShoppingListItem>()
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    public async Task<ShoppingListItem?> AddItemAsync(ShoppingListItem item)
    {
        await _context.Set<ShoppingListItem>().AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }
}