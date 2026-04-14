using System.Security.AccessControl;
using Vela.Application.Common;
using Vela.Application.DTOs.ShoppingList;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities.ShoppingList;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class ShoppingListService(IShoppingListRepository shoppingListRepository,
    IIngredientRepository ingredientRepository, IMealPlanRepository mealPlanRepository) : IShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly IIngredientRepository _ingredientRepository = ingredientRepository;
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    
    public async Task<Result<ShoppingListDto>> GetShoppingListAsync(string? userId,  Guid? groupId)
    {
        
        var hasUserId = !string.IsNullOrWhiteSpace(userId);
        var hasGroupId = groupId.HasValue && groupId != Guid.Empty;
        
        if (hasUserId == hasGroupId)
            return Result<ShoppingListDto>.Fail("Shopping list must belong to either a user or a group. Not both or none");

        ShoppingList shoppingList;

        if (hasUserId)
        {
            shoppingList = await _shoppingListRepository.GetByUserIdAsync(userId);
            if (shoppingList == null)
                return Result<ShoppingListDto>.Fail("Shopping list not found");
        }
        else
        {
            Guid foundGroupId = groupId ?? Guid.Empty;
            shoppingList = await _shoppingListRepository.GetByGroupIdAsync(foundGroupId);
            if (shoppingList == null)
                return Result<ShoppingListDto>.Fail("Shopping list not found");
        }

        return Result<ShoppingListDto>.Ok(new ShoppingListDto
        {
            Id = shoppingList.Id,
            UserId =  shoppingList.UserId,
            GroupId =  shoppingList.GroupId,
            Name = shoppingList.Name,
            CreatedAt =  shoppingList.CreatedAt,
            UpdatedAt =  shoppingList.UpdatedAt,
            Items = shoppingList.Items?.Select(i => new ShoppingListItemDto
            {
                Id = i.Id,
                IngredientName = i.IngredientName,
                AssignedUserId = i.AssignedUserId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Price = i.Price,
                Shop = i.Shop,
                IsBought = i.IsBought,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList() ?? new()
        });
    }

    public async Task<Result<ShoppingListDto?>> GetShoppingListById(Guid id)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        
        if (shoppingList == null)
            return Result<ShoppingListDto?>.Fail("ShoppingList not found");

        var dto = new ShoppingListDto
        {
            Id = shoppingList.Id,
            UserId = shoppingList.UserId,
            GroupId = shoppingList.GroupId,
            Name = shoppingList.Name,
            CreatedAt = shoppingList.CreatedAt,
            UpdatedAt = shoppingList.UpdatedAt,
            Items = shoppingList.Items?.Select(i => new ShoppingListItemDto
            {
                Id = i.Id,
                IngredientName = i.IngredientName,
                AssignedUserId = i.AssignedUserId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Price = i.Price,
                Shop = i.Shop,
                IsBought = i.IsBought,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList() ?? new()
        };
        
        return Result<ShoppingListDto?>.Ok(dto);
    }

    public async Task<Result<ShoppingListDto>> CreateShoppingListAsync(string? userId, Guid? groupId, string name)
    {
        var hasUserId = !string.IsNullOrWhiteSpace(userId);
        var hasGroupId = groupId.HasValue && groupId != Guid.Empty;
        
        if (hasUserId == hasGroupId)
            return Result<ShoppingListDto>.Fail("Shopping list must belong to either a user or a group. Not both or none");


        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = groupId,
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _shoppingListRepository.AddAsync(shoppingList);
        await _shoppingListRepository.SaveChangesAsync();

        return Result<ShoppingListDto>.Ok(new ShoppingListDto
        {
            Id = shoppingList.Id,
            UserId = shoppingList.UserId,
            GroupId = shoppingList.GroupId,
            Name = shoppingList.Name,
            CreatedAt = shoppingList.CreatedAt,
            UpdatedAt = shoppingList.UpdatedAt,
            Items = new()
        });
    }

    public async Task<Result<ShoppingListItemDto>> UpdateShoppingListItem(Guid itemId, ShoppingListItemDto dto )
    {
        var item = await _shoppingListRepository.GetItemByIdAsync(itemId);
        if (item == null)
            return Result<ShoppingListItemDto>.Fail("Item not found");
        
        item.IngredientName = dto.IngredientName;
        item.AssignedUserId = dto.AssignedUserId;
        item.Quantity = dto.Quantity;
        item.Unit = dto.Unit;
        item.Price = dto.Price;
        item.Shop = dto.Shop;
        item.IsBought = dto.IsBought;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _shoppingListRepository.SaveChangesAsync();
        return Result<ShoppingListItemDto>.Ok(dto);
    } 
    
    public async Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, string userId, AddShoppingListItemDto dto)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId);
        if (shoppingList == null)
            return Result<ShoppingListItemDto>.Fail("Shopping list not found");
        
        var (normalizedQty, normalizedUnit) = UnitConverter.Normalize(dto.Quantity, dto.Unit);
        
        ShoppingListItem item;

        if (dto.IngredientId.HasValue && dto.IngredientId != Guid.Empty)
        {
            var ingredient = await _ingredientRepository.GetByUuidAsync(dto.IngredientId);
            if (ingredient == null)
                return Result<ShoppingListItemDto>.Fail("Provided Ingredient not found");
            
            item = new ShoppingListItem
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                ItemCategory = ingredient.Category,
                Quantity = normalizedQty,
                Unit = normalizedUnit,
            };
        }
        else
        {
            item = new ShoppingListItem
            {
                IngredientId = dto.IngredientId,
                IngredientName = dto.IngredientName,
                ItemCategory = dto.Category,
                Quantity = normalizedQty,
                Unit = normalizedUnit,
            };
        }
        
        shoppingList.Items.Add(item);
        await _shoppingListRepository.AddItemAsync(item);

        shoppingList.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _shoppingListRepository.SaveChangesAsync();

        return Result<ShoppingListItemDto>.Ok(
            new ShoppingListItemDto
            {
                Id = item.Id,
                IngredientName = item.IngredientName,
                AssignedUserId = item.AssignedUserId,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Price = item.Price,
                Shop = item.Shop,
                IsBought = item.IsBought,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            });
    }

    public async Task<Result> DeleteShoppingListAsync(Guid id)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        if (shoppingList == null)
            return Result.Fail("Shopping list not found");

        await _shoppingListRepository.DeleteAsync(shoppingList);
        await _shoppingListRepository.SaveChangesAsync();
    
        return Result.Ok();
    }

    public async Task<Result> DeleteItemAsync(Guid itemId)
    {
        var item = await _shoppingListRepository.DeleteItemAsync(itemId);
        if (item == null)
            return Result.Fail("Item not found");
        await _shoppingListRepository.SaveChangesAsync();

        return Result.Ok();
    }
    
    public async Task<Result<ShoppingListDto?>> GenerateFromMealPlanAsync(
        Guid mealPlanId, DateOnly startDate, DateOnly endDate)
    {
        var mealPlan = await _mealPlanRepository.GetByIdWithEntriesByDateRangeAsync(mealPlanId, startDate, endDate);
        if (mealPlan == null)
            return Result<ShoppingListDto?>.Fail("Meal plan not found");

        ShoppingList shoppingList;

        if (mealPlan.GroupId.HasValue)
        {
            var groupShoppingList = await _shoppingListRepository.GetByGroupIdAsync(mealPlan.GroupId.Value);
            if (groupShoppingList == null)
                return Result<ShoppingListDto?>.Fail("Group shopping list not found");
            shoppingList = groupShoppingList;
        }
        else
        {
            var userShoppingList = await _shoppingListRepository.GetByUserIdAsync(mealPlan.UserId);
            if (userShoppingList == null)
                return Result<ShoppingListDto?>.Fail("User shopping list not found");
            
            shoppingList = userShoppingList;
        }
        

        foreach (var entry in mealPlan.Entries)
        {
            if (entry.AddedToShoppingList) 
                continue;
            
            var servingBase = entry.Recipe.ServingSize > 0 ? entry.Recipe.ServingSize : 1;
            var scaleFactor = (double)entry.Servings / servingBase;

            var recipeTotals = new Dictionary<(
                Guid IngredientId, 
                string Name, 
                IngredientCategory Category,
                string? Unit)
                , double>();
            
            foreach (var recipeIngred in entry.Recipe.Ingredients)
            {
                var (normalizedQty, normalizedUnit) =
                    UnitConverter.Normalize(recipeIngred.Quantity * scaleFactor, recipeIngred.Unit);
                
                var normalizedName = recipeIngred.Ingredient.Name.Trim().ToLowerInvariant();

                var key = (
                    Id: recipeIngred.IngredientId, 
                    Name: normalizedName, 
                    Category: recipeIngred.Ingredient.Category, 
                    UnitConverter: normalizedUnit);
                
                recipeTotals[key] = recipeTotals.TryGetValue(key, out var existingQty)
                    ? existingQty + normalizedQty
                    : normalizedQty;
            }

            foreach (var kvp in recipeTotals)
            {
                var newItem = new ShoppingListItem
                {
                    Id = Guid.NewGuid(),
                    ShoppingListId = shoppingList.Id,
                    IngredientId = kvp.Key.IngredientId,
                    IngredientName = kvp.Key.Name,
                    ItemCategory =  kvp.Key.Category,
                    MealPlanEntryId = entry.Id,
                    Quantity = kvp.Value,
                    Unit = kvp.Key.Unit,
                    IsBought = false
                };
                await _shoppingListRepository.AddItemAsync(newItem);
            }
            entry.AddedToShoppingList = true;
        }
        await _shoppingListRepository.SaveChangesAsync();
        return await GetShoppingListById(shoppingList.Id);
    }
    
    public async Task<Result> AssignItemToUserAsync(Guid itemId, string targetUserId)
    {
        var item = await _shoppingListRepository.GetItemByIdAsync(itemId);
        if (item == null) return Result.Fail("Item not found");

        // Opdater hvem varen er tildelt
        item.AssignedUserId = targetUserId;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _shoppingListRepository.SaveChangesAsync();
        return Result.Ok();
    }
    
    public async Task<Result> ClearAllItemsAsync(Guid shoppingListId)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId);
    
        if (shoppingList == null)
            return Result.Fail("Indkøbslisten blev ikke fundet.");

        if (!shoppingList.Items.Any())
            return Result.Ok();
        
        await _shoppingListRepository.RemoveRangeAsync(shoppingList.Items);
        await _shoppingListRepository.SaveChangesAsync(); // Eller _shoppingListRepository.SaveChangesAsync()

        return Result.Ok();
    }

    public async Task<Result> ClearPurchasedItemsAsync(Guid shoppingListId)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId);
    
        if (shoppingList == null)
            return Result.Fail("Indkøbslisten blev ikke fundet.");
        
        var itemsToRemove = shoppingList.Items.Where(i => i.IsBought).ToList();

        if (!itemsToRemove.Any())
            return Result.Ok();

        await _shoppingListRepository.RemoveRangeAsync(itemsToRemove);
        await _shoppingListRepository.SaveChangesAsync();

        return Result.Ok();
    }
}