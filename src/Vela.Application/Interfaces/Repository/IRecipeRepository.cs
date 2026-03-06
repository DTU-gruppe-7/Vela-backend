using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<Recipe?> GetByIdWithIngredientsAsync(Guid id);
    Task<IEnumerable<Recipe>> GetAllSummariesAsync();
    Task<IEnumerable<Recipe>> GetNextRecipesAsync(string userId, int limit);
}