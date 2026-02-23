using Microsoft.AspNetCore.Mvc;
using System;
using Vela.Application.Interfaces.External;
using Vela.Application.Interfaces.Service;

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
	}
}