using Vela.Domain.Entities.Recipes;

namespace Vela.Application.Interfaces.Repository;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<Recipe?> GetByIdWithIngredientsAsync(Guid id);
    Task<IEnumerable<Recipe>> GetAllSummariesAsync();
    Task<IEnumerable<Recipe>> GetNextRecipesAsync(string userId, int limit, string? category = null);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<Recipe>> GetMostLikedRecipesAsync(int limit = 20);
}