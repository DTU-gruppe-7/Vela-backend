using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Recipe>> GetAllSummariesAsync()
    {
        return await _dbSet
            .Select(r => new Recipe
            {
                Id = r.Id,
                Name = r.Name,
                Category = r.Category,
                ThumbnailUrl = r.ThumbnailUrl,
                WorkTime = r.WorkTime,
                TotalTime = r.TotalTime,
                KeywordsJson = r.KeywordsJson
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdWithIngredientsAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet
            .AnyAsync(r => r.Name.ToLower() == name.ToLower());
    }
    
    public async Task<IEnumerable<Recipe>> GetNextRecipesAsync(Guid userId, int limit)
    {
        return await _dbSet
            .Where(r => !_context.Set<SwipeRecipe>().Any(sr => sr.RecipeId == r.Id && sr.UserId == userId))
            .OrderBy(r => r.Id)
            .Select(r => new Recipe
            {
                Id = r.Id,
                Name = r.Name,
                Category = r.Category,
                ThumbnailUrl = r.ThumbnailUrl,
                WorkTime = r.WorkTime,
                TotalTime = r.TotalTime,
                KeywordsJson = r.KeywordsJson
            })
            .AsNoTracking()
            .Take(limit)
            .ToListAsync();
    }
}