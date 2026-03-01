using Vela.Application.DTOs;
using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Service;

public interface IRecipeService
{
    Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(Guid recipeId);
    Task<IEnumerable<Recipe>> GetNextRecipesAsync(Guid userId, int limit);
}