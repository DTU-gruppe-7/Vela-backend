using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class SwipeRepository : ISwipeRepository
{
    private readonly AppDbContext _context;
    public SwipeRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task RecordSwipeAsync(SwipeRecipe swipe)
    {
        await _context.Set<SwipeRecipe>().AddAsync(swipe);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> HasUserSwipedOnRecipeAsync(Guid userId, Guid recipeId)
    {
        return await _context.Set<SwipeRecipe>()
            .AnyAsync(sr => sr.UserId == userId && sr.RecipeId == recipeId);
    }
}