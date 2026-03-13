using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Domain.Enums;
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
    
    public async Task<IEnumerable<Recipe>> GetNextRecipesAsync(string userId, int limit, string? category = null)

    {
        var query = _dbSet
            .Where(r => !_context.Set<SwipeRecipe>().Any(sr => sr.RecipeId == r.Id && sr.UserId == userId));
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(r => r.Category == category);
        }
        return await query
            .OrderBy(r => r.Id)
            .Take(limit)
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
    
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Recipes
            .Where(r => r.Category != null && r.Category != "")
            .Select(r => r.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IEnumerable<Recipe>> GetMostLikedRecipesAsync(int limit = 20)
    {
        limit = Math.Clamp(limit, 1, 100);

        return await _context.Set<SwipeRecipe>()
            .Where(sr => sr.Direction == SwipeDirection.Like)
            .GroupBy(sr => sr.RecipeId)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(limit)
            .Join(_dbSet,
                g => g.Key,
                r => r.Id,
                (g, r) => new Recipe
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
}