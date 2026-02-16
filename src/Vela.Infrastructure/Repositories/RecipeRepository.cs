using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(AppDbContext context) : base(context)
    {}

    public async Task<bool> ExistsByExternalIdAsync(string externalId)
    {
        return await _dbSet
            .AnyAsync(r => r.ExternalId == externalId);
    }

    public async Task<IEnumerable<Recipe>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(r => r.Category == category)
            .ToListAsync();
    }
}