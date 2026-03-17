using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

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

        var shoppingList = new ShoppingList();

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

    public async Task<Result<ShoppingListDto>> GetShoppingListById(Guid id)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        
        if (shoppingList == null)
            return Result<ShoppingListDto>.Fail("ShoppingList not found");

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
        
        var normalizedName = dto.IngredientName.Trim().ToLowerInvariant();
        var (normalizedQty, normalizedUnit) = UnitConverter.Normalize(dto.Quantity, dto.Unit);
        
        var (item, isNew) = AccumulateOrCreateItem(shoppingList, normalizedName, normalizedUnit, normalizedQty);

        if (isNew)
        {
            shoppingList.Items.Add(item);
            await _shoppingListRepository.AddItemAsync(item);
        }
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

    public async Task<Result<ShoppingListDto>> UpdateShoppingListAsync(Guid id, UpdateShoppingListDto dto)
    {
        var shoppingList = await _shoppingListRepository.GetByUuidAsync(id);
        if (shoppingList == null)
            return Result<ShoppingListDto>.Fail("Shopping list not found");

        if (dto.Name != null)
            shoppingList.Name = dto.Name;

        shoppingList.UpdatedAt = DateTimeOffset.UtcNow;

        await _shoppingListRepository.UpdateAsync(shoppingList);
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
        Guid mealPlanId,
        string currentUserId,
        Guid? targetShoppingListId = null,
        Guid? groupId = null)
    {
        var mealPlan = await _mealPlanRepository.GetByIdWithEntriesAsync(mealPlanId);
        if (mealPlan == null)
            return Result<ShoppingListDto?>.Fail("Mealplan not found");

        ShoppingList shoppingList;

        if (targetShoppingListId.HasValue)
        {
            var found = await _shoppingListRepository.GetByIdWithItemsAsync(targetShoppingListId.Value);
            if (found == null)
                return Result<ShoppingListDto?>.Fail("Existing shopping list not found");
            shoppingList = found;
        }
        else
        {
            shoppingList = new ShoppingList
            {
                Id = Guid.NewGuid(),
                UserId = string.IsNullOrWhiteSpace(currentUserId) ? null : currentUserId,
                GroupId = groupId,
                Name = $"Indkøb til {mealPlan.Name}",
                CreatedAt = DateTimeOffset.UtcNow,
                Items = new List<ShoppingListItem>()
            };

            await _shoppingListRepository.AddAsync(shoppingList);
        }

        shoppingList.Items ??= new List<ShoppingListItem>();

        // 1) Aggregate all normalized ingredient totals from meal plan
        var totals = new Dictionary<(String IngredientName, string? Unit), double>();

        foreach (var entry in mealPlan.Entries)
        {
            var servingBase = entry.Recipe.ServingSize > 0 ? entry.Recipe.ServingSize : 1;
            var scaleFactor = (double)entry.Servings / servingBase;

            foreach (var recipeIngred in entry.Recipe.Ingredients)
            {
                var (normalizedQty, normalizedUnit) =
                    UnitConverter.Normalize(recipeIngred.Quantity * scaleFactor, recipeIngred.Unit);
                
                var normalizedName = recipeIngred.Ingredient.Name.Trim().ToLowerInvariant();  

                var key = (normalizedName, normalizedUnit);
                totals[key] = totals.TryGetValue(key, out var existingQty)
                    ? existingQty + normalizedQty
                    : normalizedQty;
            }
        }
        
        // 2) Apply aggregate totals to existing tracked items or create new ones
        foreach (var kvp in totals)
        {
            var ingredientName = kvp.Key.IngredientName;
            var unit = kvp.Key.Unit;
            var qtyToAdd = kvp.Value;
            
            var (item, isNew) = AccumulateOrCreateItem(shoppingList, ingredientName, unit, qtyToAdd);
            if (isNew)
            {
                await _shoppingListRepository.AddItemAsync(item);
            }
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

    private (ShoppingListItem, bool isNew) AccumulateOrCreateItem(ShoppingList shoppingList, string ingredientName, string? unit, double qtyToAdd)
    {
        var existingItem = shoppingList.Items?.FirstOrDefault(i =>                                                                                                                                                                       
            i.IngredientName.Trim().Equals(ingredientName, StringComparison.OrdinalIgnoreCase) &&                                                                                                                                       
            i.Unit == unit); 

        if (existingItem != null)
        {
            existingItem.Quantity += qtyToAdd;
            existingItem.UpdatedAt = DateTimeOffset.UtcNow;
            return (existingItem, false);
        }

        return (new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            ShoppingListId = shoppingList.Id,
            IngredientName = ingredientName,
            AssignedUserId = null,
            Quantity = qtyToAdd,
            Unit = unit,
            IsBought = false,
            CreatedAt = DateTimeOffset.UtcNow
        }, true);
    }
}