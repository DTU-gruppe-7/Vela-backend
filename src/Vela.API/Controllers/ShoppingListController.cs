using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[Authorize]
public class ShoppingListController(IShoppingListService shoppingListService) : BaseApiController
{
    private readonly IShoppingListService _shoppingListService = shoppingListService;

    [HttpGet]
    public async Task<ActionResult<List<ShoppingListDto>>> GetShoppingList([FromQuery] Guid groupId)
    {
        if (groupId.Equals(Guid.Empty))
        {
            var curentUserId = GetCurrentUserId();
            var result = await _shoppingListService.GetShoppingListAsync(curentUserId, null);
            if (!result.Success)
                return NotFound(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }
        else
        {
            var result = await _shoppingListService.GetShoppingListAsync(null, groupId);
            if (!result.Success)
                return NotFound(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }

    }
    
    [HttpPatch("{id}")]
    public async Task<ActionResult<ShoppingListDto>> UpdateShoppingList(Guid id, [FromBody] UpdateShoppingListDto dto)
    {
        var result = await _shoppingListService.UpdateShoppingListAsync(id, dto);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<ShoppingListItemDto>> AddItem(Guid id, [FromBody] AddShoppingListItemDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.AddItemAsync(id, userId, dto);

        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    
    [HttpDelete("{id}/items/{itemId}")]
    public async Task<ActionResult> DeleteItem(Guid id, Guid itemId)
    {
        var result = await _shoppingListService.DeleteItemAsync(itemId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    [HttpPut("{id}/items/{itemId}")]
    public async Task<ActionResult<ShoppingListItemDto>> UpdateShoppingListItem(Guid id, Guid itemId, [FromBody] ShoppingListItemDto dto)
    {
        var result = await _shoppingListService.UpdateShoppingListItem(itemId, dto);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });
        
        return Ok(result.Data);
    }
    
    [HttpPost("from-mealplan/{mealPlanId}")]
    public async Task<ActionResult<ShoppingListDto>> AddFromMealPlan(
        Guid mealPlanId)
    {
        var result = await _shoppingListService.GenerateFromMealPlanAsync(
            mealPlanId);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }
}