using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.Common;
using Vela.Application.DTOs.ShoppingList;
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
            var callerUserId = GetCurrentUserId();
            var result = await _shoppingListService.GetShoppingListAsync(callerUserId, null, callerUserId);
            if (!result.Success)
                return NotFound(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }
        else
        {
            var callerUserId = GetCurrentUserId();
            var result = await _shoppingListService.GetShoppingListAsync(null, groupId, callerUserId);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                    ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                    _ => BadRequest(new { message = result.ErrorMessage })
                };
            }
            return Ok(result.Data);
        }

    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<ShoppingListItemDto>> AddItem(Guid id, [FromBody] AddShoppingListItemDto dto)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.AddItemAsync(id, dto, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return Ok(result.Data);
    }

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<ActionResult> DeleteItem(Guid id, Guid itemId)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.DeleteItemAsync(itemId, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return NoContent();
    }

    [HttpPut("{id}/items/{itemId}")]
    public async Task<ActionResult<ShoppingListItemDto>> UpdateShoppingListItem(Guid id, Guid itemId, [FromBody] ShoppingListItemDto dto)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.UpdateShoppingListItem(itemId, dto, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return Ok(result.Data);
    }
    
    [HttpPost("{id}/from-mealplan/{mealPlanId}")]
    public async Task<ActionResult<ShoppingListDto>> AddFromMealPlan(
        Guid id,
        Guid mealPlanId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate, 
        [FromBody] GenerateFromMealPlanRequestDto? requestDto)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }
        
        var excludedEntryIds = requestDto?.ExcludedMealPlanEntryIds ?? new List<Guid>();

        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.GenerateFromMealPlanAsync(
            mealPlanId, startDate, endDate, excludedEntryIds, callerUserId);

        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(result.Data);
    }
    
    [HttpDelete("{id}/clear")]
    public async Task<ActionResult> ClearAll(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.ClearAllItemsAsync(id, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return NoContent();
    }

    [HttpDelete("{id}/clear-purchased")]
    public async Task<ActionResult> ClearPurchased(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.ClearPurchasedItemsAsync(id, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return NoContent();
    }


    [HttpDelete("{id}/from-mealplan/{mealPlanId}")]
    public async Task<ActionResult> DeleteMealPlanEntry(
        Guid id,
        Guid mealPlanId,
        [FromQuery] Guid mealPlanEntryId)
    {
        if (mealPlanEntryId == Guid.Empty)
            return BadRequest(new { message = "There must be a meal plan entry ID provided" });
        
        var callerUserId = GetCurrentUserId();
        var result = await _shoppingListService.DeleteMealPlanEntryAsync(id, mealPlanEntryId, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(result);
    }

    [HttpPatch("{id}/items/{itemId}/assign")]
    public async Task<ActionResult> AssignItem(Guid id, Guid itemId, [FromBody] AssignItemRequestDto dto)
    {
        // Vi kalder servicen for at opdatere tildelingen i databasen
        var result = await _shoppingListService.AssignItemToUserAsync(itemId, dto.UserId);
    
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Varen er nu tildelt brugeren" });
    }
}