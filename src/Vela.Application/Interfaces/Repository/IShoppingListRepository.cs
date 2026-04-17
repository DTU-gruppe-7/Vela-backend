using Vela.Application.DTOs;
using Vela.Domain.Entities.ShoppingList;

namespace Vela.Application.Interfaces.Repository;

public interface IShoppingListRepository : IRepository<ShoppingList>
{
    Task<ShoppingList?> GetByUserIdAsync(string userId);
    Task<ShoppingList?> GetByGroupIdAsync(Guid groupId);
    Task<ShoppingList?> GetByIdWithItemsAsync(Guid id); 
    Task<ShoppingListItem?> GetItemByIdAsync(Guid id);
    Task<ShoppingListItem?> AddItemAsync(ShoppingListItem item);
    Task<ShoppingListItem?> DeleteItemAsync(Guid itemId);
    Task<ShoppingListItem?> UpdateItemAsync(ShoppingListItem item);
    Task DeleteItemsByMealPlanEntryIdAsync(Guid mealPlanEntryId);
    Task RemoveRangeAsync(IEnumerable<ShoppingListItem> items);
    Task<List<ShoppingListItem>> GetItemsAssignedToUserAsync(string userId);
}