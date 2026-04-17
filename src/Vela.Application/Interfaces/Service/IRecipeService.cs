using Vela.Application.DTOs;
using Vela.Domain.Enums;

namespace Vela.Application.Interfaces.Service;

public interface IRecipeService
{
    Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync(IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false);
    Task<RecipeDto?> GetRecipeByIdAsync(Guid recipeId);
    Task<IEnumerable<RecipeSummaryDto>> GetNextRecipesAsync(string userId, int limit, string? category = null, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<RecipeSummaryDto>> GetMostLikedRecipesAsync(int limit = 20, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false);
}