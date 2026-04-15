using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.DTOs.MealPlan;

namespace Vela.Application.Interfaces.Service;

public interface IMealPlanService
{
    Task<Result<MealPlanDto>> GetMealPlanAsync(string? userId, Guid? groupId, DateOnly startDate, DateOnly endDate);
    Task<Result<MealPlanDto>> CreateMealPlanAsync(string? userId, Guid? groupId, string name);
    Task<Result> UpdateMealPlanAsync(Guid mealPlanId, string name, string? description);
    Task<Result> DeleteMealPlanAsync(Guid mealPlanId);
    Task<Result<MealPlanEntryDto>> AddRecipeToMealPlanAsync(Guid mealPlanId, AddMealPlanEntryRequest request);
    Task<Result> RemoveRecipeFromMealPlanAsync(Guid mealPlanId, Guid entryId);
    Task<Result> UpdateMealPlanEntryServingsAsync(Guid mealPlanId, Guid entryId, int? servings, DateOnly? newDate);
    Task<Result<MealPlanDto>> GetMealPlanWithEntriesAsync(Guid mealPlanId);
    Task<Result<MealPlanDto>> GetAggregatedMealPlanAsync(string userId, DateOnly startDate, DateOnly endDate);
}
