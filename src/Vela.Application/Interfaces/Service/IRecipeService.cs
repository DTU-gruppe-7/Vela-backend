using Vela.Application.DTOs.Recipe;

namespace Vela.Application.Interfaces.Service;

public interface IRecipeService
{
    Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync();
}