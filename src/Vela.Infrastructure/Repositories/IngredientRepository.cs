using Microsoft.EntityFrameworkCore;
using Vela.Domain.Entities.Recipes;
using Vela.Infrastructure.Data;
using Vela.Application.Interfaces.Repository;

namespace Vela.Infrastructure.Repositories;

public class IngredientRepository(AppDbContext context) : Repository<Ingredient>(context), IIngredientRepository
{

    public async Task<Ingredient?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
    }
    

    public async Task<List<Ingredient>> SearchByNameAsync(string query, int limit)
    {
        query = query.Trim();

        return await _dbSet
            .AsNoTracking()
            .Where(i => EF.Functions.ILike(i.Name, $"{query}%"))
            .OrderBy(i => i.Name)
            .Take(limit)
            .ToListAsync();
    }
}