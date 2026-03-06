using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class ShoppingListService(IShoppingListRepository shoppingListRepository) : IShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
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
}