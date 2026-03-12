using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class ShoppingListService(IShoppingListRepository shoppingListRepository,
    IIngredientRepository ingredientRepository) : IShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly IIngredientRepository _ingredientRepository = ingredientRepository;
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

    public async Task<ShoppingListDto?> GetShoppingListById(Guid id)
    {
        var shoppingList = await _shoppingListRepository.GetByIdWithItemsAsync(id);
        
        if (shoppingList == null)
            return null;

        return new ShoppingListDto
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
                UserId = i.UserId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Price = i.Price,
                Shop = i.Shop,
                IsBought = i.IsBought,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList() ?? new()
        };
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
            UserId = userId,
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
            UserId = item.UserId,
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
                UserId = i.UserId,
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
            UserId = item.UserId,
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
}