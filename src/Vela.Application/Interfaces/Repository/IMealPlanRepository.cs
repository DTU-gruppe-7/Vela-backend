using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IMealPlanRepository : IRepository<MealPlan>
{
    Task<MealPlan?> GetMealPlanWithEntriesAsync(Guid mealPlanId);
    Task<MealPlan?> GetByUserIdAsync(string userId);
    Task<MealPlan?> GetByGroupIdAsync(Guid groupId);
    Task AddEntryAsync(MealPlanEntry entry);
    Task RemoveEntryAsync(Guid entryId);
    Task<MealPlanEntry?> GetEntryAsync(Guid entryId);
    Task<MealPlan?> GetByIdWithEntriesAsync(Guid id);
}
