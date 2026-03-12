using Vela.Application.Common;
using Vela.Application.DTOs.MealPlan;

namespace Vela.Application.Interfaces.Service;

public interface IMealPlanService
{
    Task<Result<MealPlanDto>> GetMealPlanAsync(Guid mealPlanId);
    Task<Result<IEnumerable<MealPlanDto>>> GetAllMealPlansAsync();
    Task<Result<IEnumerable<MealPlanDto>>> GetAllMealPlansByUserAsync(string userId);
    Task<Result<MealPlanDto>> CreateMealPlanAsync(string userId, string name, string? description = null);
    Task<Result> UpdateMealPlanAsync(Guid mealPlanId, string name, string? description);
    Task<Result> DeleteMealPlanAsync(Guid mealPlanId);
    Task<Result<MealPlanEntryDto>> AddRecipeToMealPlanAsync(Guid mealPlanId, AddMealPlanEntryRequest request);
    Task<Result> RemoveRecipeFromMealPlanAsync(Guid mealPlanId, Guid entryId);
    Task<Result> UpdateMealPlanEntryServingsAsync(Guid mealPlanId, Guid entryId, int servings);
    Task<Result<MealPlanDto>> GetMealPlanWithEntriesAsync(Guid mealPlanId);
}
