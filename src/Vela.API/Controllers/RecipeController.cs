using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class RecipeController(IRecipeService recipeService) : BaseApiController
	{
		private readonly IRecipeService _recipeService =  recipeService;
		
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var recipes = await _recipeService.GetAllRecipesAsync();
			return Ok(recipes);
		}
		
		[HttpGet("{id}")]
		public async Task<ActionResult<RecipeDto>> GetRecipeById(Guid id)
		{
			var recipe = await _recipeService.GetRecipeByIdAsync(id);
			if (recipe == null)
				return NotFound();
			return Ok(recipe);
		}
		
		[HttpGet("next")]
		public async Task<ActionResult<IEnumerable<RecipeSummaryDto>>> GetNextRecipes([FromQuery] int limit = 20)
		{
			var userid = GetCurrentUserId();
			var recipes = await _recipeService.GetNextRecipesAsync(userid, limit);
			return Ok(recipes);
		}
	}	
}