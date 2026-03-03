using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IMealPlanRepository : IRepository<MealPlan>
{
    Task<MealPlan?> GetMealPlanWithEntriesAsync(Guid mealPlanId);
    Task AddEntryAsync(MealPlanEntry entry);
    Task RemoveEntryAsync(Guid entryId);
    Task<MealPlanEntry?> GetEntryAsync(Guid entryId);
}
