using Vela.Application.Common;
using Vela.Application.DTOs.ShoppingList;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities.ShoppingList;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class ShoppingListService(IShoppingListRepository shoppingListRepository,
    IIngredientRepository ingredientRepository, IMealPlanRepository mealPlanRepository,
    IGroupRepository groupRepository, IGroupAuthorizationService groupAuthorizationService,
    IShoppingListIngredientExclusionProvider ingredientExclusionProvider) : IShoppingListService
    
{
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly IIngredientRepository _ingredientRepository = ingredientRepository;
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IGroupAuthorizationService _groupAuthorizationService = groupAuthorizationService;
    private readonly IShoppingListIngredientExclusionProvider _ingredientExclusionProvider = ingredientExclusionProvider;

    private async Task<Result> AuthorizeShoppingListAccessAsync(ShoppingList shoppingList, string callerUserId)
    {
        if (shoppingList.GroupId == null) return Result.Ok();
        var group = await _groupRepository.GetGroupWithMembersAsync(shoppingList.GroupId.Value);
        if (group == null) return Result.Fail("Group not found", ResultErrorType.NotFound);
        return _groupAuthorizationService.AuthorizeMembership(group, callerUserId);
    }

    private static string GetCanonicalUnit(string? unit) => unit?.Trim().ToLowerInvariant() switch
    {
        "g" => "g",
        "ml" => "ml",
        _ => "stk"
    };

    private static string NormalizeUnit(string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? "stk" : unit.Trim().ToLowerInvariant();

    private static string ResolveTargetUnit(string dbUnit, string recipeUnit) =>
        dbUnit == "stk" && !string.IsNullOrWhiteSpace(recipeUnit) ? recipeUnit : dbUnit;

    private static string ResolveWinningUnit(string currentUnit, string incomingUnit)
    {
        var normalizedCurrent = NormalizeUnit(currentUnit);
        var normalizedIncoming = NormalizeUnit(incomingUnit);

        if (normalizedCurrent == normalizedIncoming)
            return normalizedCurrent;

        // Business rule: if g and ml collide, g always wins.
        if (normalizedCurrent == "g" || normalizedIncoming == "g")
            return "g";
        if (normalizedCurrent == "ml" || normalizedIncoming == "ml")
            return "ml";

        if (normalizedCurrent == "stk")
            return normalizedIncoming;
        if (normalizedIncoming == "stk")
            return normalizedCurrent;

        return normalizedCurrent;
    }

    private static bool IsWeightOrVolumeUnit(string unit) => unit is "g" or "ml";

    private static (double Quantity, string Unit) ConvertToUnit(
        double quantity,
        string sourceUnit,
        string targetUnit,
        IngredientCategory category)
    {
        var normalizedSource = NormalizeUnit(sourceUnit);
        var normalizedTarget = NormalizeUnit(targetUnit);

        var (convertedQuantity, convertedUnit) =
            UnitConverter.Normalize(quantity, normalizedSource, normalizedTarget, category);

        if (convertedUnit.Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase))
            return (convertedQuantity, normalizedTarget);

        // If conversion path is not available but units are g/ml family, force 1:1 fallback.
        if (IsWeightOrVolumeUnit(normalizedSource) && IsWeightOrVolumeUnit(normalizedTarget))
        {
            var (asMlQty, asMlUnit) = UnitConverter.Normalize(quantity, normalizedSource, "ml", category);
            if (asMlUnit == "ml")
            {
                return normalizedTarget == "g" ? (asMlQty, "g") : (asMlQty, "ml");
            }
        }

        return (convertedQuantity, NormalizeUnit(convertedUnit));
    }
    
    public async Task<Result<ShoppingListDto>> GetShoppingListAsync(string? userId, Guid? groupId, string callerUserId)

    {
        var hasUserId = !string.IsNullOrWhiteSpace(userId);
        var hasGroupId = groupId.HasValue && groupId != Guid.Empty;

        if (hasUserId == hasGroupId)
            return Result<ShoppingListDto>.Fail("Shopping list must belong to either a user or a group. Not both or none");

        ShoppingList? shoppingList;
        List<ShoppingListItem> assignedItems = new();

        if (hasUserId)
        {
            shoppingList = await _shoppingListRepository.GetByUserIdAsync(userId!);
            if (shoppingList == null)

                return Result<ShoppingListDto>.Fail("Shopping list not found", ResultErrorType.NotFound);

            // Henter varer fra grupper, som er tildelt denne bruger
            assignedItems = await _shoppingListRepository.GetItemsAssignedToUserAsync(userId!);

        }
        else
        {
            Guid foundGroupId = groupId ?? Guid.Empty;
            shoppingList = await _shoppingListRepository.GetByGroupIdAsync(foundGroupId);
            if (shoppingList == null)
                return Result<ShoppingListDto>.Fail("Shopping list not found", ResultErrorType.NotFound);

            var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
            if (!authResult.Success)
                return Result<ShoppingListDto>.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);
        }

        // Kombiner de faste varer på listen med de tildelte varer (assignedItems)
        var allItems = shoppingList.Items?.ToList() ?? new List<ShoppingListItem>();
        
        foreach (var assignedItem in assignedItems)
        {
            // Vi tilføjer kun varen, hvis den ikke allerede findes på listen (undgår dubletter)
            if (allItems.All(i => i.Id != assignedItem.Id))
            {
                allItems.Add(assignedItem);
            }
        }
        
        // Byg et lookup for "fremmede" lister
        var foreignListIds = allItems
            .Where(x => x.ShoppingListId != shoppingList.Id)
            .Select(x => x.ShoppingListId)
            .Distinct()
            .ToList();

        var listNameById = new Dictionary<Guid, string?>();

        foreach (var listId in foreignListIds)
        {
            var foreignList = await _shoppingListRepository.GetByIdWithItemsAsync(listId);
            listNameById[listId] = foreignList?.Name;
        }

        return Result<ShoppingListDto>.Ok(new ShoppingListDto
        {
            Id = shoppingList.Id,
            UserId = shoppingList.UserId,
            GroupId = shoppingList.GroupId,
            Name = shoppingList.Name,
            CreatedAt = shoppingList.CreatedAt,
            UpdatedAt = shoppingList.UpdatedAt,
            Items = allItems.Select(i => new ShoppingListItemDto
            {
                Id = i.Id,
                IngredientName = i.IngredientName,
                AssignedUserId = i.AssignedUserId,
                Quantity = i.Quantity,
                // Hvis varen er tildelt fra en anden liste, markerer vi det i navnet
                RecipeName = i.ShoppingListId != shoppingList.Id
                ? $"{(
                    listNameById.GetValueOrDefault(i.ShoppingListId)?
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault() is { Length: > 0 } firstWord
                        ? $"(Fra {firstWord} gruppen)"
                        : "(Fra ukendt gruppe)"
                  )} {i.MealPlanEntry?.Recipe?.Name} ({i.MealPlanEntry?.Date})"
                : $"{i.MealPlanEntry?.Recipe?.Name} ({i.MealPlanEntry?.Date})",
                Category = i.ItemCategory,
                Unit = i.Unit,
                Price = i.Price,
                Shop = i.Shop,
                IsBought = i.IsBought,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList()
        });
    }

    public async Task<Result<ShoppingListDto?>> GetShoppingListById(Guid id)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        
        if (shoppingList == null)
            return Result<ShoppingListDto?>.Fail("ShoppingList not found", ResultErrorType.NotFound);

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
                Category = i.ItemCategory,
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

    public async Task<Result<ShoppingListItemDto>> UpdateShoppingListItem(Guid itemId, ShoppingListItemDto dto, string callerUserId)
    {
        var item = await _shoppingListRepository.GetItemByIdAsync(itemId);
        if (item == null)
            return Result<ShoppingListItemDto>.Fail("Item not found", ResultErrorType.NotFound);

        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(item.ShoppingListId);
        if (shoppingList == null)
            return Result<ShoppingListItemDto>.Fail("Shopping list not found", ResultErrorType.NotFound);

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result<ShoppingListItemDto>.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);
        
        item.IngredientName = dto.IngredientName;
        item.AssignedUserId = dto.AssignedUserId;
        item.Quantity = dto.Quantity;
        item.ItemCategory = dto.Category;
        item.Unit = dto.Unit;
        item.Price = dto.Price;
        item.Shop = dto.Shop;
        item.IsBought = dto.IsBought;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _shoppingListRepository.SaveChangesAsync();
        return Result<ShoppingListItemDto>.Ok(dto);
    } 
    
    public async Task<Result<ShoppingListItemDto>> AddItemAsync(Guid shoppingListId, AddShoppingListItemDto dto, string callerUserId)
    {
        if (dto.Quantity <= 0)
            return Result<ShoppingListItemDto>.Fail("Quantity must be greater than zero");

        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId);
        if (shoppingList == null)
            return Result<ShoppingListItemDto>.Fail("Shopping list not found", ResultErrorType.NotFound);

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result<ShoppingListItemDto>.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);
        
        ShoppingListItem item;

        if (dto.IngredientId.HasValue && dto.IngredientId != Guid.Empty)
        {
            var ingredient = await _ingredientRepository.GetByUuidAsync(dto.IngredientId);
            if (ingredient == null)
                return Result<ShoppingListItemDto>.Fail("Ingredient not found", ResultErrorType.NotFound);
            
            var (normalizedQty, normalizedUnit) = UnitConverter.Normalize(dto.Quantity, dto.Unit, ingredient.Unit, ingredient.Category);
            
            item = new ShoppingListItem
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                ItemCategory = ingredient.Category,
                Quantity = normalizedQty,
                Unit = normalizedUnit,
                AssignedUserId = dto.AssignedUserId,
            };
        }
        else
        {
            var (normalizedQty, normalizedUnit) = UnitConverter.Normalize(dto.Quantity, dto.Unit, null, null);
            
            item = new ShoppingListItem
            {
                IngredientId = dto.IngredientId,
                IngredientName = dto.IngredientName,
                ItemCategory = dto.Category,
                Quantity = normalizedQty,
                Unit = normalizedUnit,
                AssignedUserId = dto.AssignedUserId,
            };
        }
        
        shoppingList.Items!.Add(item);
        await _shoppingListRepository.AddItemAsync(item);

        shoppingList.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _shoppingListRepository.SaveChangesAsync();

        return Result<ShoppingListItemDto>.Ok(
            new ShoppingListItemDto
            {
                Id = item.Id,
                IngredientName = item.IngredientName,
                AssignedUserId = item.AssignedUserId,
                Category = item.ItemCategory,
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
            return Result.Fail("Shopping list not found", ResultErrorType.NotFound);

        await _shoppingListRepository.DeleteAsync(shoppingList);
        await _shoppingListRepository.SaveChangesAsync();
    
        return Result.Ok();
    }

    public async Task<Result> DeleteItemAsync(Guid itemId, string callerUserId)
    {
        var item = await _shoppingListRepository.GetItemByIdAsync(itemId);
        if (item == null)
            return Result.Fail("Item not found", ResultErrorType.NotFound);

        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(item.ShoppingListId);
        if (shoppingList == null)
            return Result.Fail("Shopping list not found", ResultErrorType.NotFound);

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        await _shoppingListRepository.DeleteItemAsync(itemId);
        await _shoppingListRepository.SaveChangesAsync();

        return Result.Ok();
    }
    
    public async Task<Result<ShoppingListDto?>> GenerateFromMealPlanAsync(
        Guid mealPlanId, DateOnly startDate, DateOnly endDate, IReadOnlyCollection<Guid>? excludedEntryIds, string callerUserId)
    {
        var mealPlan = await _mealPlanRepository.GetByIdWithEntriesByDateRangeAsync(mealPlanId, startDate, endDate);
        if (mealPlan == null)
            return Result<ShoppingListDto?>.Fail("Meal plan not found", ResultErrorType.NotFound);

        ShoppingList shoppingList;

        if (mealPlan.GroupId.HasValue)
        {
            var groupShoppingList = await _shoppingListRepository.GetByGroupIdAsync(mealPlan.GroupId.Value);
            if (groupShoppingList == null)
                return Result<ShoppingListDto?>.Fail("Group shopping list not found", ResultErrorType.NotFound);
            shoppingList = groupShoppingList;
        }
        else
        {
            var userShoppingList = await _shoppingListRepository.GetByUserIdAsync(mealPlan.UserId!);
            if (userShoppingList == null)
                return Result<ShoppingListDto?>.Fail("User shopping list not found", ResultErrorType.NotFound);

            shoppingList = userShoppingList;
        }

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result<ShoppingListDto?>.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        var excludedIds = excludedEntryIds is { Count: > 0 }
            ? excludedEntryIds.ToHashSet()
            : new HashSet<Guid>();

        foreach (var entry in mealPlan.Entries)
        {
            if (excludedIds.Contains(entry.Id))
                continue;
            if (entry.AddedToShoppingList) 
                continue;
            
            if (entry.Recipe?.Ingredients == null || !entry.Recipe.Ingredients.Any())
                continue;
            
            var servingBase = entry.Recipe.ServingSize > 0 ? entry.Recipe.ServingSize : 1;
            var scaleFactor = (double)entry.Servings / servingBase;

            var recipeTotals = new Dictionary<string, (
                Guid? IngredientId,
                string OriginalName,
                IngredientCategory Category,
                string Unit,
                double Quantity)>();
            
            foreach (var recipeIngred in entry.Recipe.Ingredients)
            {
                var originalName = recipeIngred.Ingredient.Name;
                if (_ingredientExclusionProvider.IsExcluded(originalName))
                    continue;

                var normalizedNameForKey = originalName.Trim().ToLowerInvariant();
                var dbUnit = GetCanonicalUnit(recipeIngred.Ingredient.Unit);
                var recipeUnit = NormalizeUnit(recipeIngred.Unit);
                var targetUnit = ResolveTargetUnit(dbUnit, recipeUnit);

                var (quantityInTargetUnit, normalizedTargetUnit) = ConvertToUnit(
                    recipeIngred.Quantity * scaleFactor,
                    recipeUnit,
                    targetUnit,
                    recipeIngred.Ingredient.Category);

                if (recipeTotals.TryGetValue(normalizedNameForKey, out var existingValue))
                {
                    var winningUnit = ResolveWinningUnit(existingValue.Unit, normalizedTargetUnit);

                    var (existingQuantityInWinningUnit, _) = ConvertToUnit(
                        existingValue.Quantity,
                        existingValue.Unit,
                        winningUnit,
                        existingValue.Category);

                    var (incomingQuantityInWinningUnit, _) = ConvertToUnit(
                        quantityInTargetUnit,
                        normalizedTargetUnit,
                        winningUnit,
                        recipeIngred.Ingredient.Category);

                    recipeTotals[normalizedNameForKey] = (
                        existingValue.IngredientId,
                        existingValue.OriginalName,
                        existingValue.Category,
                        winningUnit,
                        existingQuantityInWinningUnit + incomingQuantityInWinningUnit);
                }
                else
                {
                    recipeTotals[normalizedNameForKey] = (
                        recipeIngred.IngredientId,
                        originalName,
                        recipeIngred.Ingredient.Category,
                        normalizedTargetUnit,
                        quantityInTargetUnit);
                }
            }

            foreach (var kvp in recipeTotals)
            {
                var newItem = new ShoppingListItem
                {
                    Id = Guid.NewGuid(),
                    ShoppingListId = shoppingList.Id,
                    IngredientId = kvp.Value.IngredientId,
                    IngredientName = kvp.Value.OriginalName,
                    ItemCategory = kvp.Value.Category,
                    MealPlanEntryId = entry.Id,
                    Quantity = kvp.Value.Quantity,
                    Unit = kvp.Value.Unit,
                    IsBought = false
                };
                await _shoppingListRepository.AddItemAsync(newItem);
            }
            entry.AddedToShoppingList = true;
        }
        await _shoppingListRepository.SaveChangesAsync();
        return await GetShoppingListById(shoppingList.Id);
    }

    public async Task<Result> DeleteMealPlanEntryAsync(Guid id, Guid mealPlanEntryId, string callerUserId)
    {
        if (id == Guid.Empty)
            return Result.Fail("Shopping list ID is required");
        var mealPlanEntry = await _mealPlanRepository.GetEntryAsync(mealPlanEntryId);
        if (mealPlanEntry == null)
            return Result.Fail("Meal plan entry not found", ResultErrorType.NotFound);

        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        if (shoppingList == null)
            return Result.Fail("Shopping list not found", ResultErrorType.NotFound);

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        var hasMatchingItems = shoppingList.Items?.Any(i => i.MealPlanEntryId == mealPlanEntryId) ?? false;
        if (!hasMatchingItems)
        {
            mealPlanEntry.AddedToShoppingList = false;
            await _shoppingListRepository.SaveChangesAsync();
            return Result.Ok();
        }
        
        await _shoppingListRepository.DeleteItemsByMealPlanEntryIdAsync(mealPlanEntryId);
        mealPlanEntry.AddedToShoppingList = false;
        
        await _shoppingListRepository.SaveChangesAsync();
        return Result.Ok();
    }
    
    public async Task<Result> AssignItemToUserAsync(Guid itemId, string targetUserId)
    {
        var item = await _shoppingListRepository.GetItemByIdAsync(itemId);
        if (item == null) return Result.Fail("Item not found", ResultErrorType.NotFound);

        item.AssignedUserId = targetUserId;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _shoppingListRepository.SaveChangesAsync();
        return Result.Ok();
    }
    
    public async Task<Result> ClearAllItemsAsync(Guid shoppingListId, string callerUserId)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId);

        if (shoppingList == null)
            return Result.Fail("Indkøbslisten blev ikke fundet.", ResultErrorType.NotFound);

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        if (!(shoppingList.Items?.Any() ?? false))
            return Result.Ok();

        await _shoppingListRepository.RemoveRangeAsync(shoppingList.Items!);
        await _shoppingListRepository.SaveChangesAsync(); 

        return Result.Ok();
    }

    public async Task<Result> ClearPurchasedItemsAsync(Guid shoppingListId, string callerUserId)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(shoppingListId);

        if (shoppingList == null)
            return Result.Fail("Indkøbslisten blev ikke fundet.", ResultErrorType.NotFound);

        var authResult = await AuthorizeShoppingListAccessAsync(shoppingList, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);
        
        var itemsToRemove = shoppingList.Items?.Where(i => i.IsBought).ToList() ?? [];

        if (!itemsToRemove.Any())
            return Result.Ok();

        await _shoppingListRepository.RemoveRangeAsync(itemsToRemove);
        await _shoppingListRepository.SaveChangesAsync();

        return Result.Ok();
    }
}



