using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Enums;

namespace Vela.API.Controllers
{
	[Authorize]
	public class RecipeController(IRecipeService recipeService, IAuthService authService) : BaseApiController
	{
		private readonly IRecipeService _recipeService = recipeService;
		private readonly IAuthService _authService = authService;

	[HttpGet]
	public async Task<IActionResult> GetAll([FromQuery] string? exclude = null)
	{
		if (!TryParseExcludedAllergens(exclude, out var excludedAllergens, out var errorResult))
			return BadRequest(new { message = errorResult });

		var dietaryPreferencesResult = await _authService.GetDietaryPreferencesAsync(GetCurrentUserId());
		if (!dietaryPreferencesResult.Success)
			return NotFound(new { message = dietaryPreferencesResult.ErrorMessage });

		MergeStoredPreferences(excludedAllergens, dietaryPreferencesResult.Data!, out var mergedAllergens, out var requireVeganRecipes);

		var recipes = await _recipeService.GetAllRecipesAsync(mergedAllergens, requireVeganRecipes);
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
		[FromQuery] string? category = null,
		[FromQuery] string? exclude = null)
	{
		if (!TryParseExcludedAllergens(exclude, out var excludedAllergens, out var errorResult))
			return BadRequest(new { message = errorResult });

		var dietaryPreferencesResult = await _authService.GetDietaryPreferencesAsync(GetCurrentUserId());
		if (!dietaryPreferencesResult.Success)
			return NotFound(new { message = dietaryPreferencesResult.ErrorMessage });

		MergeStoredPreferences(excludedAllergens, dietaryPreferencesResult.Data!, out var mergedAllergens, out var requireVeganRecipes);

		var userid = GetCurrentUserId();
		var recipes = await _recipeService.GetNextRecipesAsync(userid, limit, category, mergedAllergens, requireVeganRecipes);
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
			[FromQuery] int limit = 20,
			[FromQuery] string? exclude = null)
		{
			if (!TryParseExcludedAllergens(exclude, out var excludedAllergens, out var errorResult))
				return BadRequest(new { message = errorResult });

			var recipes = await _recipeService.GetMostLikedRecipesAsync(limit, excludedAllergens);
			return Ok(recipes);
		}

		private static bool TryParseExcludedAllergens(
			string? rawValue,
			out IReadOnlyCollection<RecipeAllergen> excludedAllergens,
			out string? errorMessage)
		{
			if (string.IsNullOrWhiteSpace(rawValue))
			{
				excludedAllergens = Array.Empty<RecipeAllergen>();
				errorMessage = null;
				return true;
			}

			var parsed = new HashSet<RecipeAllergen>();
			var tokens = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			foreach (var token in tokens)
			{
				switch (token.ToLowerInvariant())
				{
					case "gluten":
						parsed.Add(RecipeAllergen.Gluten);
						break;
					case "lactose":
						parsed.Add(RecipeAllergen.Lactose);
						break;
					case "nuts":
						parsed.Add(RecipeAllergen.Nuts);
						break;
					default:
						excludedAllergens = Array.Empty<RecipeAllergen>();
						errorMessage = $"Unknown allergen '{token}'. Supported values: gluten, lactose, nuts.";
						return false;
				}
			}

			excludedAllergens = parsed.ToArray();
			errorMessage = null;
			return true;
		}

		private static void MergeStoredPreferences(
			IReadOnlyCollection<RecipeAllergen> explicitAllergens,
			UserDietaryPreferencesDto preferences,
			out IReadOnlyCollection<RecipeAllergen> mergedAllergens,
			out bool requireVeganRecipes)
		{
			var merged = new HashSet<RecipeAllergen>(explicitAllergens);

			if (preferences.AvoidGluten)
				merged.Add(RecipeAllergen.Gluten);

			if (preferences.AvoidLactose)
				merged.Add(RecipeAllergen.Lactose);

			if (preferences.AvoidNuts)
				merged.Add(RecipeAllergen.Nuts);

			mergedAllergens = merged.ToArray();
			requireVeganRecipes = preferences.IsVegan;
		}
	}
}