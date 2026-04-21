using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs.ShoppingList;
using Vela.Application.Interfaces.Repository;

namespace Vela.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
public class IngredientsController(IIngredientRepository ingredientRepository) : BaseApiController
{
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<IngredientLookupDto>>> SearchAsync(
        [FromQuery] string query,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(Array.Empty<IngredientLookupDto>());
        
        var ingredients = await ingredientRepository.SearchByNameAsync(query,  limit);
        
        return Ok(ingredients.Select(i => new IngredientLookupDto
        {
            Id = i.Id,
            Name = i.Name,
            Unit = i.Unit,
            Category = i.Category,
        }));
    }
}