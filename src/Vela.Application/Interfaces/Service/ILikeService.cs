using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Service;

public interface ILikeService
{
    Task<Result> RecordSwipeAsync(string userId, SwipeDto swipeDto);

    Task <IEnumerable<RecipeSummaryDto>> GetLikedRecipesByUserIdAsync(string userId);

    Task RecalculateGroupMatchesAsync(Group group);
    Task RecordGroupMatch(Guid groupId, Guid recipeId);
}


