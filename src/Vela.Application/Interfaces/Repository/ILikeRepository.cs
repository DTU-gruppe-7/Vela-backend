using Vela.Domain.Entities;
using Vela.Domain.Entities.Recipe;

namespace Vela.Application.Interfaces.Repository;

public interface ILikeRepository : IRepository<Like>
{
    Task<bool> HasUserSwipedOnRecipeAsync(string userId, Guid recipeId);
    Task <IEnumerable<Recipe>> GetLikedRecipesByUserIdAsync(string userId);
    Task<IEnumerable<Guid>> GetCommonLikedRecipeIdsAsync(IEnumerable<string> userIds);
    Task<bool> CheckForNewMatch(IEnumerable<string> userIds, Guid recipeId);
    Task RecordMatchAsync(Match match);
    Task DeleteMatchesByGroupIdAsync(Guid groupId);
}
