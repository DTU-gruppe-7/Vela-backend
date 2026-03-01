using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;
using RecipeSummaryDto = Vela.Application.DTOs.Recipe.RecipeSummaryDto;

namespace Vela.Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;

    public RecipeService(IRecipeRepository recipeRepository)
    {
        _recipeRepository = recipeRepository;
    }

    public async Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository.GetAllSummariesAsync();

        return recipes.Select(r => new RecipeSummaryDto
        {
            Id = r.Id,
            Name = r.Name,
            Category = r.Category,
            ThumbnailUrl = r.ThumbnailUrl,
            WorkTime = r.WorkTime,
            TotalTime = r.TotalTime,
            KeywordsJson = r.KeywordsJson,
        });
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid recipeId)
    {
        return await _recipeRepository.GetByUuidAsync(recipeId);
    }

    public async Task<IEnumerable<Recipe>> GetNextRecipesAsync(Guid userId, int limit)
    {
        return await _recipeRepository.GetNextRecipesAsync(userId, limit);
    }
}