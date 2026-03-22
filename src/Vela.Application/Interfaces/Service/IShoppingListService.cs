using Vela.Application.Common;
using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.Service;

public interface IShoppingListService
{
    Task<Result<ShoppingListDto>> GetShoppingListAsync(string? userId, Guid? groupId);
    Task<Result<ShoppingListDto?>> GetShoppingListById(Guid id);
    Task<Result<ShoppingListDto>> CreateShoppingListAsync(string? userId, Guid? groupId, string name);
    Task<Result<ShoppingListItemDto>> UpdateShoppingListItem(Guid itemId, ShoppingListItemDto dto);
    Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, string userId, AddShoppingListItemDto dto);
    Task<Result> DeleteItemAsync(Guid itemId);
    Task<Result> DeleteShoppingListAsync(Guid id);
    Task<Result<ShoppingListDto>> UpdateShoppingListAsync(Guid id, UpdateShoppingListDto dto);
    Task<Result<ShoppingListDto?>> GenerateFromMealPlanAsync(Guid mealPlanId);
    Task<Result> AssignItemToUserAsync(Guid itemId, string targetUserId);
}   