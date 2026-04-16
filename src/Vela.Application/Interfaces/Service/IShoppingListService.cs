using Vela.Application.Common;
using Vela.Application.DTOs.ShoppingList;

namespace Vela.Application.Interfaces.Service;

public interface IShoppingListService
{
    Task<Result<ShoppingListDto>> GetShoppingListAsync(string? userId, Guid? groupId, string callerUserId);
    Task<Result<ShoppingListDto?>> GetShoppingListById(Guid id);
    Task<Result<ShoppingListDto>> CreateShoppingListAsync(string? userId, Guid? groupId, string name);
    Task<Result<ShoppingListItemDto>> UpdateShoppingListItem(Guid itemId, ShoppingListItemDto dto, string callerUserId);
    Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, AddShoppingListItemDto dto, string callerUserId);
    Task<Result> DeleteItemAsync(Guid itemId, string callerUserId);
    Task<Result> DeleteShoppingListAsync(Guid id);
    Task<Result<ShoppingListDto?>> GenerateFromMealPlanAsync(Guid mealPlanId, DateOnly startDate, DateOnly endDate, IReadOnlyCollection<Guid>? excludedEntryIds, string callerUserId);
    Task<Result> DeleteMealPlanEntryAsync(Guid id, Guid mealPlanEntryId, string callerUserId);
    Task<Result> AssignItemToUserAsync(Guid itemId, string targetUserId);
    Task<Result> ClearAllItemsAsync(Guid shoppingListId, string callerUserId);
    Task<Result> ClearPurchasedItemsAsync(Guid shoppingListId, string callerUserId);
}   