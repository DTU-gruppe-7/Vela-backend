using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;

namespace Vela.Application.Interfaces.Repository;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<Recipe?> GetByIdWithIngredientsAsync(Guid id);
    Task<IEnumerable<Recipe>> GetAllSummariesAsync(IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false);
    Task<IEnumerable<Recipe>> GetNextRecipesAsync(string userId, int limit, string? category = null, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<Recipe>> GetMostLikedRecipesAsync(int limit = 20, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false);
}