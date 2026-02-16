using Microsoft.EntityFrameworkCore;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;
using Vela.Application.Interfaces.Repository;

namespace Vela.Infrastructure.Repositories;

public class IngredientRepository : Repository<Ingredient>, IIngredientRepository
{
    public IngredientRepository(AppDbContext context) : base(context)
    {}

    public async Task<Ingredient> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
    }
}