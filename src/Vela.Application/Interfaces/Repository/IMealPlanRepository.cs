using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IMealPlanRepository : IRepository<MealPlan>
{
    Task<MealPlan?> GetMealPlanWithEntriesAsync(Guid mealPlanId);
    Task<IEnumerable<MealPlan>> GetByUserIdAsync(string userId);
    Task AddEntryAsync(MealPlanEntry entry);
    Task RemoveEntryAsync(Guid entryId);
    Task<MealPlanEntry?> GetEntryAsync(Guid entryId);
    
    Task UpdateEntryServingsAsync(Guid mealPlanId, Guid entryId, int servings);
}
