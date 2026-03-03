using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class SwipeService : ISwipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ISwipeRepository _swipeRepository;

    public SwipeService(IRecipeRepository recipeRepository, ISwipeRepository swipeRepository)
    {
        _recipeRepository = recipeRepository;
        _swipeRepository = swipeRepository;
    }

    public async Task<Result> RecordSwipeAsync(Guid userId, SwipeDto swipeDto)
    {
        var recipe = await _recipeRepository.GetByUuidAsync(swipeDto.RecipeId);
        if (recipe == null)
        {
            return Result.Fail("Recipe not found");
        }

        var alreadySwiped = await _swipeRepository.HasUserSwipedOnRecipeAsync(userId, swipeDto.RecipeId);
        if (alreadySwiped)
            return Result.Ok();

        var swipe = new SwipeRecipe
        {
            SwipeId = Guid.NewGuid(),
            UserId = userId,
            RecipeId = swipeDto.RecipeId,
            Direction = swipeDto.Direction,
            SwipedAt = DateTimeOffset.UtcNow,
        };
        await _swipeRepository.RecordSwipeAsync(swipe);

        return Result.Ok();
    }
    
    public async Task<IEnumerable<RecipeSummaryDto>> GetLikedRecipesByUserIdAsync(Guid userId)
    {
        var likedRecipes = await _swipeRepository.GetLikedRecipesByUserIdAsync(userId);
        return likedRecipes.Select(r => new RecipeSummaryDto
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
}
