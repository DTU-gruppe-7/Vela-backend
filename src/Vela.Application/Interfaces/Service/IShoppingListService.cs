using Vela.Application.Common;
using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.Service;

public interface IShoppingListService
{
    Task<IEnumerable<ShoppingListSummaryDto>> GetAllShoppingListsAsync();
    Task<Result<ShoppingListDto?>> GetShoppingListById(Guid id);
    Task<ShoppingListDto> CreateShoppingListAsync(string userId, CreateShoppingListDto dto);
    Task<Result> MarkItemAsBoughtAsync(Guid itemId);
    Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, string userId, AddShoppingListItemDto dto);
    Task<Result<ShoppingListDto?>> GenerateFromMealPlanAsync(Guid mealPlanId, string currentUserId, Guid? targetShoppingListId = null, Guid? groupId = null);
    Task<Result> AssignItemToUserAsync(Guid itemId, string targetUserId);
}   