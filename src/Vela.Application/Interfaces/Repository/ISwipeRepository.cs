using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface ISwipeRepository
{
    Task RecordSwipeAsync(SwipeRecipe swipe);
    Task<bool> HasUserSwipedOnRecipeAsync(string userId, Guid recipeId);
    Task <IEnumerable<Recipe>> GetLikedRecipesByUserIdAsync(string userId);
}
