using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;
using RecipeSummaryDto = Vela.Application.DTOs.Recipe.RecipeSummaryDto;

namespace Vela.Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ISwipeRepository _swipeRepository;

    public RecipeService(IRecipeRepository recipeRepository, ISwipeRepository swipeRepository)
    {
        _recipeRepository = recipeRepository;
        _swipeRepository = swipeRepository;

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

    public async Task RecordSwipeAsync(Guid userId, SwipeDto swipeDto)
    {
        var recipe = await _recipeRepository.GetByUuidAsync(swipeDto.RecipeId);
        if (recipe == null)
        {
            throw new Exception("Recipe not found");
        }

        var alreadySwiped = await _swipeRepository.HasUserSwipedOnRecipeAsync(userId, swipeDto.RecipeId);
        if (alreadySwiped)
            return;

        var swipe = new SwipeRecipe
        {
            SwipeId = Guid.NewGuid(),
            UserId = userId,
            RecipeId = swipeDto.RecipeId,
            Direction = swipeDto.Direction,
            SwipedAt = DateTime.UtcNow,
        };
        await _swipeRepository.RecordSwipeAsync(swipe);
    }
}