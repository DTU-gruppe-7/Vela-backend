using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Domain.Enums;
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
    public async Task<bool> HasUserSwipedOnRecipeAsync(string userId, Guid recipeId)
    {
        return await _context.Set<SwipeRecipe>()
            .AnyAsync(sr => sr.UserId == userId && sr.RecipeId == recipeId);
    }
    public async Task<IEnumerable<Recipe>> GetLikedRecipesByUserIdAsync(string userId)
    {
        return await _context.Set<SwipeRecipe>()
            .Where(sr => sr.UserId == userId && sr.Direction == SwipeDirection.Like)
            .Select(sr => sr.Recipe)
            .ToListAsync();
    }
}