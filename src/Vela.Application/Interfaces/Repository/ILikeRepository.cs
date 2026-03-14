using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface ILikeRepository : IRepository<Like>
{
    Task<bool> HasUserSwipedOnRecipeAsync(string userId, Guid recipeId);
    Task <IEnumerable<Recipe>> GetLikedRecipesByUserIdAsync(string userId);
    Task<IEnumerable<Guid>> GetCommonLikedRecipeIdsAsync(IEnumerable<string> userIds);
    Task RecordMatchAsync(Match match);
    Task DeleteMatchesByGroupIdAsync(Guid groupId);
}
