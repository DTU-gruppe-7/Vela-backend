using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class SwipeService(IRecipeRepository recipeRepository, ILikeRepository likeRepository) : ISwipeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository;
    private readonly ILikeRepository _likeRepository  = likeRepository;

    public async Task<Result> RecordSwipeAsync(string userId, SwipeDto swipeDto)
    {
        var recipe = await _recipeRepository.GetByUuidAsync(swipeDto.RecipeId);
        if (recipe == null)
        {
            return Result.Fail("Recipe not found");
        }

        var alreadySwiped = await _likeRepository.HasUserSwipedOnRecipeAsync(userId, swipeDto.RecipeId);
        if (alreadySwiped)
            return Result.Ok();

        var swipe = new Like
        {
            LikeId = Guid.NewGuid(),
            UserId = userId,
            RecipeId = swipeDto.RecipeId,
            Direction = swipeDto.Direction,
            SwipedAt = DateTimeOffset.UtcNow,
        };
        await _likeRepository.AddAsync(swipe);
        await _likeRepository.SaveChangesAsync();

        return Result.Ok();
    }
    
    public async Task<IEnumerable<RecipeSummaryDto>> GetLikedRecipesByUserIdAsync(string userId)
    {
        var likedRecipes = await _likeRepository.GetLikedRecipesByUserIdAsync(userId);
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
