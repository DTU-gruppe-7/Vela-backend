using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class MealPlanRepository : Repository<MealPlan>, IMealPlanRepository
{
    public MealPlanRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<MealPlan?> GetMealPlanWithEntriesAsync(Guid mealPlanId)
    {
        return await _dbSet
            .Include(mp => mp.Entries)
            .ThenInclude(mpe => mpe.Recipe)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);
    }

    public async Task<IEnumerable<MealPlan>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(mp => mp.UserId == userId)
            .Include(mp => mp.Entries)
            .ThenInclude(mpe => mpe.Recipe)
            .ToListAsync();
    }

    public async Task AddEntryAsync(MealPlanEntry entry)
    {
        await _context.MealPlanEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveEntryAsync(Guid entryId)
    {
        var entry = await _context.MealPlanEntries.FindAsync(entryId);
        if (entry != null)
        {
            _context.MealPlanEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<MealPlanEntry?> GetEntryAsync(Guid entryId)
    {
        return await _context.MealPlanEntries
            .Include(mpe => mpe.Recipe)
            .FirstOrDefaultAsync(mpe => mpe.Id == entryId);
    }

    public async Task UpdateEntryServingsAsync(Guid mealPlanId, Guid entryId, int servings)
    {
        var entry = await _context.MealPlanEntries.FindAsync(entryId);
        if (entry != null && entry.MealPlanId == mealPlanId)
        {
            entry.Servings = servings;
            await _context.SaveChangesAsync();
        }
    }
}
