using Vela.Domain.Entities.MealPlan;

namespace Vela.Application.Interfaces.Repository;

public interface IMealPlanRepository : IRepository<MealPlan>
{
    Task<MealPlan?> GetMealPlanWithEntriesAsync(Guid mealPlanId);
    Task<MealPlan?> GetByUserIdAsync(string userId);
    Task<MealPlan?> GetByGroupIdAsync(Guid groupId);
    Task AddEntryAsync(MealPlanEntry entry);
    Task RemoveEntryAsync(Guid entryId);
    Task<MealPlanEntry?> GetEntryAsync(Guid entryId);
    Task<MealPlan?> GetByIdWithEntriesByDateRangeAsync(Guid id, DateOnly startDate, DateOnly endDate);
    Task<IEnumerable<MealPlan>> GetAllGroupMealPlans(IEnumerable<Guid> groupIds); 
}
