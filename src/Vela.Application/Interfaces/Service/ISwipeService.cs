using Vela.Application.Common;
using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.Service;

public interface ISwipeService
{
    Task<Result> RecordSwipeAsync(Guid userId, SwipeDto swipeDto);
    
    Task <IEnumerable<RecipeSummaryDto>> GetLikedRecipesByUserIdAsync(Guid userId);
}


