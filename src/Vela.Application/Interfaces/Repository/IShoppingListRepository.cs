using Vela.Application.DTOs;
using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IShoppingListRepository : IRepository<ShoppingList>
{
    Task<ShoppingList?> GetByIdWithItemsAsync(Guid id); 
    Task<IEnumerable<ShoppingList>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<ShoppingList>> GetByGroupId(Guid groupId);
    Task<ShoppingListItem?> GetItemByIdAsync(Guid id);
    Task<ShoppingListItem?> AddItemAsync(ShoppingListItem item);
}