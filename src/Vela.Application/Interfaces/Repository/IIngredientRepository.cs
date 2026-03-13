using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IIngredientRepository : IRepository<Ingredient>
{
    Task<Ingredient> GetByNameAsync(string name);
    Task<Ingredient> GetByIdAsync(Guid ingredientId);
}