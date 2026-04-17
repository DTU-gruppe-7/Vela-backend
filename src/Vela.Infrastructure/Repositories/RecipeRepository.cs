using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class RecipeRepository(AppDbContext context) : Repository<Recipe>(context), IRecipeRepository
{
    public async Task<IEnumerable<Recipe>> GetAllSummariesAsync(IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false)
    {
        var query = ApplyDietaryFilters(_dbSet.AsNoTracking(), excludedAllergens, requireVeganRecipes);

        return await query
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
    
    public async Task<IEnumerable<Recipe>> GetNextRecipesAsync(string userId, int limit, string? category = null, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false)

    {
        var query = ApplyDietaryFilters(_dbSet, excludedAllergens, requireVeganRecipes)
            .Where(r => !_context.Set<Like>().Any(sr => sr.RecipeId == r.Id && sr.UserId == userId));
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

    public async Task<IEnumerable<Recipe>> GetMostLikedRecipesAsync(int limit = 20, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false)
    {
        limit = Math.Clamp(limit, 1, 100);

        var recipeQuery = ApplyDietaryFilters(_dbSet, excludedAllergens, requireVeganRecipes);

        return await _context.Set<Like>()
            .Where(sr => sr.Direction == SwipeDirection.Like)
            .GroupBy(sr => sr.RecipeId)
            .Select(g => new
            {
                RecipeId = g.Key,
                Likes = g.Count()
            })
            .Join(recipeQuery,
                x => x.RecipeId,
                r => r.Id,
                (x, r) => new
                {
                    x.Likes,
                    Recipe = r
                })
            .OrderByDescending(x => x.Likes)
            .Take(limit)
            .Select(x => new Recipe
            {
                Id = x.Recipe.Id,
                Name = x.Recipe.Name,
                Category = x.Recipe.Category,
                ThumbnailUrl = x.Recipe.ThumbnailUrl,
                WorkTime = x.Recipe.WorkTime,
                TotalTime = x.Recipe.TotalTime,
                KeywordsJson = x.Recipe.KeywordsJson
            })
            .AsNoTracking()
            .ToListAsync();
    }

    private static IQueryable<Recipe> ApplyDietaryFilters(
        IQueryable<Recipe> query,
        IReadOnlyCollection<RecipeAllergen>? excludedAllergens,
        bool requireVeganRecipes)
    {
        if (excludedAllergens == null || excludedAllergens.Count == 0)
        {
            if (!requireVeganRecipes)
                return query;
        }

        if (excludedAllergens != null && excludedAllergens.Count > 0)
        {
            if (excludedAllergens.Contains(RecipeAllergen.Gluten))
                query = query.Where(r => !r.Ingredients.Any(ri => ri.Ingredient.ContainsGluten));

            if (excludedAllergens.Contains(RecipeAllergen.Lactose))
                query = query.Where(r => !r.Ingredients.Any(ri => ri.Ingredient.ContainsLactose));

            if (excludedAllergens.Contains(RecipeAllergen.Nuts))
                query = query.Where(r => !r.Ingredients.Any(ri => ri.Ingredient.ContainsNuts));
        }

        if (requireVeganRecipes)
            query = query.Where(r => !r.Ingredients.Any(ri => !ri.Ingredient.IsVegan));

        return query;
    }
}