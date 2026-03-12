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
    
    public async Task<IEnumerable<ShoppingListSummaryDto>> GetAllShoppingListsAsync()
    {
        var shoppingLists = await _shoppingListRepository.GetAllAsync();

        return shoppingLists.Select(r => new ShoppingListSummaryDto()
        {
            Id = r.Id,
            UserId =  r.UserId,
            GroupId =  r.GroupId,
            Name = r.Name,
            CreatedAt =  r.CreatedAt,
            UpdatedAt =  r.UpdatedAt,
        }).ToList();
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
                IngredientId = i.IngredientId,
                IngredientName = i.Ingredient.Name,
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

    public async Task<ShoppingListDto> CreateShoppingListAsync(string userId, CreateShoppingListDto dto)
    {
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = dto.GroupId,
            Name = dto.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _shoppingListRepository.AddAsync(shoppingList);

        return new ShoppingListDto
        {
            Id = shoppingList.Id,
            UserId = shoppingList.UserId,
            GroupId = shoppingList.GroupId,
            Name = shoppingList.Name,
            CreatedAt = shoppingList.CreatedAt,
            UpdatedAt = shoppingList.UpdatedAt,
            Items = new()
        };
    }

    public async Task<Result> MarkItemAsBoughtAsync(Guid itemId)
    {
        var item = await _shoppingListRepository.GetItemByIdAsync(itemId);
        if (item == null)
            return Result.Fail("Item not found");
        
        item.IsBought = true;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _shoppingListRepository.SaveChangesAsync();
        return Result.Ok();
    } 
    
    public async Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, string userId, AddShoppingListItemDto dto)
    {
        var shoppingList = await _shoppingListRepository.GetByUuidAsync(shoppingListId);
        if (shoppingList == null)
            return Result<ShoppingListItemDto>.Fail("Shopping list not found");

        var ingredient = await _ingredientRepository.GetByUuidAsync(dto.IngredientId);
        if (ingredient == null)
            return Result<ShoppingListItemDto>.Fail("Ingredient not found");
        
        var item = new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            ShoppingListId = shoppingListId,
            ShoppingList = shoppingList,
            IngredientId = dto.IngredientId,
            Ingredient = ingredient,
            AssignedUserId = dto.AssignedUserId,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Price = dto.Price,
            Shop = dto.Shop,
            IsBought = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _shoppingListRepository.AddItemAsync(item);

        var itemDto = new ShoppingListItemDto
        {
            Id = item.Id,
            IngredientId = item.IngredientId,
            IngredientName = ingredient.Name,
            AssignedUserId = item.AssignedUserId,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Price = item.Price,
            Shop = item.Shop,
            IsBought = item.IsBought,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };

        return Result<ShoppingListItemDto>.Ok(itemDto);
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

    public async Task<Result<ShoppingListDto>> DeleteShoppingListAsync(Guid id)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        if (shoppingList == null)
            return Result<ShoppingListDto>.Fail("Shopping list not found");

        await _shoppingListRepository.DeleteAsync(id);

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
                IngredientId = i.IngredientId,
                IngredientName = i.Ingredient?.Name ?? string.Empty,
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

        return Result<ShoppingListDto>.Ok(dto);
    }

    public async Task<Result<ShoppingListItemDto>> DeleteItemAsync(Guid itemId)
    {
        var item = await _shoppingListRepository.DeleteItemAsync(itemId);
        if (item == null)
            return Result<ShoppingListItemDto>.Fail("Item not found");

        var itemDto = new ShoppingListItemDto
        {
            Id = item.Id,
            IngredientId = item.IngredientId,
            IngredientName = item.Ingredient?.Name ?? string.Empty,
            AssignedUserId = item.AssignedUserId,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Price = item.Price,
            Shop = item.Shop,
            IsBought = item.IsBought,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };

        return Result<ShoppingListItemDto>.Ok(itemDto);
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
        var totals = new Dictionary<(Guid IngredientId, string? Unit), double>();

        foreach (var entry in mealPlan.Entries)
        {
            var servingBase = entry.Recipe.ServingSize > 0 ? entry.Recipe.ServingSize : 1;
            var scaleFactor = (double)entry.Servings / servingBase;

            foreach (var recipeIngred in entry.Recipe.Ingredients)
            {
                var (normalizedQty, normalizedUnit) =
                    UnitConverter.Normalize(recipeIngred.Quantity * scaleFactor, recipeIngred.Unit);

                var key = (recipeIngred.IngredientId, normalizedUnit);
                totals[key] = totals.TryGetValue(key, out var existingQty)
                    ? existingQty + normalizedQty
                    : normalizedQty;
            }
        }

        // 2) Pre-load all needed ingredients
        var ingredientIds = totals.Keys.Select(k => k.IngredientId).Distinct();
        var ingredientLookup = new Dictionary<Guid, Ingredient>();
        foreach (var id in ingredientIds)
        {
            var ingredient = await _ingredientRepository.GetByUuidAsync(id);
            if (ingredient != null)
                ingredientLookup[id] = ingredient;
        }

        // 3) Apply aggregate totals to existing tracked items or create new ones
        foreach (var kvp in totals)
        {
            var ingredientId = kvp.Key.IngredientId;
            var unit = kvp.Key.Unit;
            var qtyToAdd = kvp.Value;

            var existingItem = shoppingList.Items.FirstOrDefault(i =>
                i.IngredientId == ingredientId &&
                i.Unit == unit);

            if (existingItem != null)
            {
                existingItem.Quantity += qtyToAdd;
                existingItem.UpdatedAt = DateTimeOffset.UtcNow;
                continue;
            }

            if (!ingredientLookup.TryGetValue(ingredientId, out var ingredientEntity))
                continue; // skip if ingredient not found

            shoppingList.Items.Add(new ShoppingListItem
            {
                Id = Guid.NewGuid(),
                ShoppingListId = shoppingList.Id,
                ShoppingList = shoppingList,
                IngredientId = ingredientId,
                Ingredient = ingredientEntity,          // ← required member now set
                AssignedUserId = string.IsNullOrWhiteSpace(currentUserId) ? null : currentUserId,
                Quantity = qtyToAdd,
                Unit = unit,
                IsBought = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
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
}