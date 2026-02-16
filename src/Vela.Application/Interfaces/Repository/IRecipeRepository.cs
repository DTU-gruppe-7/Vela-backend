using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<bool> ExistsByExternalIdAsync(string externalId);
    Task<IEnumerable<Recipe>> GetByCategoryAsync(string category);
}