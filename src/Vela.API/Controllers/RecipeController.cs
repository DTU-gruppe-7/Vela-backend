using Microsoft.AspNetCore.Mvc;
using System;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RecipeController : ControllerBase
	{
		private readonly IRecipeService _recipeService;

		public RecipeController(IRecipeService recipeService)
		{
			_recipeService = recipeService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var recipes = await _recipeService.GetAllRecipesAsync();
			return Ok(recipes);
		}


		[HttpGet("{id}")]
		public async Task<ActionResult<Recipe>> GetRecipeById(Guid id)
		{
			var recipe = await _recipeService.GetRecipeByIdAsync(id);
			if (recipe == null)
				return NotFound();
			return Ok(recipe);
		}

		[HttpGet("next")]
		public async Task<ActionResult<IEnumerable<Recipe>>> GetNextRecipes([FromQuery] int limit = 20)
		{
			var userid = GetcurrentUserId();
			var recipes = await _recipeService.GetNextRecipesAsync(userid, limit);
			return Ok(recipes);
		}

		[HttpPost("swipe")]
		public async Task<IActionResult> RecordSwipe([FromBody] SwipeDto swipeDto)
		{
			var userId = GetcurrentUserId();
			await _recipeService.RecordSwipeAsync(userId, swipeDto);
			return Ok();
		}

		private Guid GetcurrentUserId()
		{
			// In a real application, you would extract the user ID from the JWT token or session
			// For this example, we'll just return a fixed GUID for testing purposes
			return Guid.Parse("00000000-0000-0000-0000-000000000001");
		}
	}
}