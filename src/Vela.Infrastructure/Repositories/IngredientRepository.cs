using Microsoft.EntityFrameworkCore;
using Vela.Domain.Entities.Recipe;
using Vela.Infrastructure.Data;
using Vela.Application.Interfaces.Repository;

namespace Vela.Infrastructure.Repositories;

public class IngredientRepository(AppDbContext context) : Repository<Ingredient>(context), IIngredientRepository
{

    public async Task<Ingredient> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
    }

    public async Task<Ingredient?> GetByIdAsync(Guid ingredientId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Id == ingredientId);
    }
}