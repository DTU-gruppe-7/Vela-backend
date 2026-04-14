using Vela.Application.Common;
using Vela.Application.DTOs.ShoppingList;

namespace Vela.Application.Interfaces.Service;

public interface IShoppingListService
{
    Task<Result<ShoppingListDto>> GetShoppingListAsync(string? userId, Guid? groupId);
    Task<Result<ShoppingListDto?>> GetShoppingListById(Guid id);
    Task<Result<ShoppingListDto>> CreateShoppingListAsync(string? userId, Guid? groupId, string name);
    Task<Result<ShoppingListItemDto>> UpdateShoppingListItem(Guid itemId, ShoppingListItemDto dto);
    Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, AddShoppingListItemDto dto);
    Task<Result> DeleteItemAsync(Guid itemId);
    Task<Result> DeleteShoppingListAsync(Guid id);
    Task<Result<ShoppingListDto?>> GenerateFromMealPlanAsync(Guid mealPlanId, DateOnly startDate, DateOnly endDate, IReadOnlyCollection<Guid>? excludedEntryIds = null);
    Task<Result> DeleteMealPlanEntryAsync(Guid id, Guid mealPlanEntryId);
    Task<Result> AssignItemToUserAsync(Guid itemId, string targetUserId);
    Task<Result> ClearAllItemsAsync(Guid shoppingListId);
    Task<Result> ClearPurchasedItemsAsync(Guid shoppingListId);
}   