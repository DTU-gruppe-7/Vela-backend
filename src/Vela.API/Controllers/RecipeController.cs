using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers
{
	[Authorize]
	public class RecipeController(IRecipeService recipeService) : BaseApiController
	{
		private readonly IRecipeService _recipeService = recipeService;

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
		public async Task<ActionResult<IEnumerable<RecipeSummaryDto>>> GetNextRecipes(
			[FromQuery] int limit = 20,
			[FromQuery] string? category = null)
		{
			var userid = GetCurrentUserId();
			var recipes = await _recipeService.GetNextRecipesAsync(userid, limit, category);
			return Ok(recipes);
		}

		[HttpGet("categories")]
		public async Task<ActionResult<IEnumerable<string>>> GetCategories()
		{
			var categories = await _recipeService.GetCategoriesAsync();
			return Ok(categories);
		}

		[AllowAnonymous]
		[HttpGet("most-liked")]
		public async Task<ActionResult<IEnumerable<RecipeSummaryDto>>> GetMostLikedRecipes(
			[FromQuery] int limit = 20)
		{
			var recipes = await _recipeService.GetMostLikedRecipesAsync(limit);
			return Ok(recipes);
		}
	}
}