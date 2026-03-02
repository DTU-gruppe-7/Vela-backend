using Vela.Application.DTOs.MealPlan;

namespace Vela.Application.Interfaces.Service;

public interface IMealPlanService
{
    Task<MealPlanDto?> GetMealPlanAsync(Guid mealPlanId);
    Task<IEnumerable<MealPlanDto>> GetAllMealPlansAsync();
    Task<MealPlanDto> CreateMealPlanAsync(string name, string? description = null);
    Task UpdateMealPlanAsync(Guid mealPlanId, string name, string? description);
    Task DeleteMealPlanAsync(Guid mealPlanId);
    Task<MealPlanEntryDto> AddRecipeToMealPlanAsync(Guid mealPlanId, AddMealPlanEntryRequest request);
    Task RemoveRecipeFromMealPlanAsync(Guid mealPlanId, Guid entryId);
    Task<MealPlanDto?> GetMealPlanWithEntriesAsync(Guid mealPlanId);
}
