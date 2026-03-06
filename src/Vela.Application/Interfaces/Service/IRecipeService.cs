using Vela.Application.DTOs;
using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Service;

public interface IRecipeService
{
    Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync();
    Task<RecipeDto?> GetRecipeByIdAsync(Guid recipeId);
    Task<IEnumerable<RecipeSummaryDto>> GetNextRecipesAsync(string userId, int limit);
}