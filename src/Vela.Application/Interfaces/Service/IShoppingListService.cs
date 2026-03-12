using Vela.Application.Common;
using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.Service;

public interface IShoppingListService
{
    Task<IEnumerable<ShoppingListSummaryDto>> GetAllShoppingListsAsync();
    Task<ShoppingListDto?> GetShoppingListById(Guid id);
    Task<ShoppingListDto> CreateShoppingListAsync(string userId, CreateShoppingListDto dto);
    Task<Result> MarkItemAsBoughtAsync(Guid itemId);
    Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, string userId, AddShoppingListItemDto dto);
    Task<Result<ShoppingListItemDto>> DeleteItemAsync(Guid itemId);
    Task<Result<ShoppingListDto>> DeleteShoppingListAsync(Guid id);
    Task<Result<ShoppingListDto>> UpdateShoppingListAsync(Guid id, UpdateShoppingListDto dto);
}   