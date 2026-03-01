using Vela.Application.DTOs;
using Vela.Domain.Entities;
using Vela.Domain.Enums;
using RecipeSummaryDto = Vela.Application.DTOs.Recipe.RecipeSummaryDto;

namespace Vela.Application.Interfaces.Service;

public interface IRecipeService
{
    Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(Guid userId);
    Task<IEnumerable<Recipe>> GetNextRecipesAsync(Guid userId, int limit);
}