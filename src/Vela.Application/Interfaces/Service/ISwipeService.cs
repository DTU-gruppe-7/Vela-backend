using Vela.Application.Common;
using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.Service;

public interface ISwipeService
{
    Task<Result> RecordSwipeAsync(string userId, SwipeDto swipeDto);
    
    Task <IEnumerable<RecipeSummaryDto>> GetLikedRecipesByUserIdAsync(string userId);
}


