using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShoppingListController(IShoppingListService shoppingListService) : BaseApiController
{
    private readonly IShoppingListService _shoppingListService = shoppingListService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var shoppingList = await _shoppingListService.GetAllShoppingListsAsync();
        return Ok(shoppingList);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShoppingListDto>> GetShoppingListById(Guid id)
    {
        var shoppingList = await _shoppingListService.GetShoppingListById(id);
        if (shoppingList == null)
            return NotFound();
        return Ok(shoppingList);
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingListDto>> Create([FromBody] CreateShoppingListDto dto)
    {
        var userId = GetCurrentUserId();
        var shoppingList = await _shoppingListService.CreateShoppingListAsync(userId, dto);
        return CreatedAtAction(nameof(GetShoppingListById), new { id = shoppingList.Id }, shoppingList);
    }
    
    [HttpPatch("{id}")]
    public async Task<ActionResult<ShoppingListDto>> UpdateShoppingList(Guid id, [FromBody] UpdateShoppingListDto dto)
    {
        var result = await _shoppingListService.UpdateShoppingListAsync(id, dto);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ShoppingListDto>> DeleteShoppingList(Guid id)
    {
        var result = await _shoppingListService.DeleteShoppingListAsync(id);
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

        return CreatedAtAction(nameof(GetShoppingListById), new { id }, result.Data);
    }

    
    [HttpDelete("{id}/items/{itemId}")]
    public async Task<ActionResult<ShoppingListItemDto>> DeleteItem(Guid id, Guid itemId)
    {
        var result = await _shoppingListService.DeleteItemAsync(itemId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPatch("{id}/items/{itemId}/bought")]
    public async Task<IActionResult> MarkItemAsBoughtAsync(Guid id, Guid itemId)
    {
        var result = await _shoppingListService.MarkItemAsBoughtAsync(itemId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });
        
        return NoContent();
    }
    
    [HttpPost("from-mealplan/{mealPlanId}")]
    public async Task<ActionResult<ShoppingListDto>> AddFromMealPlan(
        Guid mealPlanId, 
        [FromQuery] Guid? existingListId = null, 
        [FromQuery] Guid? groupId = null)
    {
        var userId = string.Empty;
        
        if (groupId == null)
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
        }
        
        var result = await _shoppingListService.GenerateFromMealPlanAsync(
            mealPlanId, 
            userId, 
            existingListId, 
            groupId);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }
}