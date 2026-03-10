using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

//[Authorize]
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
    
    [HttpPost("{id}/items")]
    public async Task<ActionResult<ShoppingListItemDto>> AddItem(Guid id, [FromBody] AddShoppingListItemDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.AddItemAsync(id, userId, dto);

        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetShoppingListById), new { id }, result.Data);
    }

    
    [HttpPatch("items/{itemId}/bought")]
    public async Task<IActionResult> MarkItemAsBoughtAsync(Guid itemId)
    {
        var result = await _shoppingListService.MarkItemAsBoughtAsync(itemId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });
        
        return NoContent();
    }
}