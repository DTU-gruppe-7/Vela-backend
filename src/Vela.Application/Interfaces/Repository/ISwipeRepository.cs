using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface ISwipeRepository
{
    Task RecordSwipeAsync(SwipeRecipe swipe);
    Task<bool> HasUserSwipedOnRecipeAsync(Guid userId, Guid recipeId);
}
