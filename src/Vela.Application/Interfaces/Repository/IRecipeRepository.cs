using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<Recipe>> GetAllSummariesAsync();
    Task<IEnumerable<Recipe>> GetNextRecipesAsync(Guid userId, int limit);
}