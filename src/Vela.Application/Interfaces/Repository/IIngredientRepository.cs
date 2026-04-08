using Vela.Domain.Entities.Recipe;

namespace Vela.Application.Interfaces.Repository;

public interface IIngredientRepository : IRepository<Ingredient>
{
    Task<Ingredient?> GetByNameAsync(string name);
    Task<List<Ingredient>> SearchByNameAsync(string query, int limit);
}