using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities.MealPlan;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class MealPlanRepository(AppDbContext context) : Repository<MealPlan>(context), IMealPlanRepository
{

    public async Task<MealPlan?> GetMealPlanWithEntriesAsync(Guid mealPlanId)
    {
        return await _dbSet
            .Include(mp => mp.Entries)
            .ThenInclude(mpe => mpe.Recipe)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);
    }

    public async Task<MealPlan?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(mp => mp.Entries)
            .ThenInclude(mpe => mpe.Recipe)
            .SingleOrDefaultAsync(mp => mp.UserId == userId);
    }
    
    public async Task<MealPlan?> GetByGroupIdAsync(Guid groupId)
    {
        return await _dbSet
            .Include(mp => mp.Entries)
            .ThenInclude(mpe => mpe.Recipe)
            .SingleOrDefaultAsync(mp => mp.GroupId == groupId);
    }

    public async Task AddEntryAsync(MealPlanEntry entry)
    {
        await _context.MealPlanEntries.AddAsync(entry);
    }

    public async Task RemoveEntryAsync(Guid entryId)
    {
        var entry = await _context.MealPlanEntries.FindAsync(entryId);
        if (entry != null)
        {
            _context.MealPlanEntries.Remove(entry);
        }
    }

    public async Task<MealPlanEntry?> GetEntryAsync(Guid entryId)
    {
        return await _context.MealPlanEntries
            .Include(mpe => mpe.Recipe)
            .FirstOrDefaultAsync(mpe => mpe.Id == entryId);
    }

    public async Task<MealPlan?> GetByIdWithEntriesByDateRangeAsync(Guid id, DateOnly startDate, DateOnly endDate)
    {
        return await _context.MealPlans
            .AsSplitQuery()
            .Include(mp => mp.Entries.Where(
                e => e.Date >= startDate && e.Date <= endDate))
            .ThenInclude(e => e.Recipe)
            .ThenInclude(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }
}
