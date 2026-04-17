using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class LikeRepository(AppDbContext context) : Repository<Like>(context), ILikeRepository 
{
    public async Task<bool> HasUserSwipedOnRecipeAsync(string userId, Guid recipeId)
    {
        return await _context.Set<Like>()
            .AnyAsync(sr => sr.UserId == userId && sr.RecipeId == recipeId);
    }
    public async Task<IEnumerable<Recipe>> GetLikedRecipesByUserIdAsync(string userId)
    {
        return await _context.Set<Like>()
            .Where(sr => sr.UserId == userId && sr.Direction == SwipeDirection.Like)
            .Select(sr => sr.Recipe)
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetCommonLikedRecipeIdsAsync(IEnumerable<string> userIds)
    {
        var usersId = userIds.ToList();
        var userCount = usersId.Count;
        
        return await _context.Set<Like>()
            .Where(sr => usersId.Contains(sr.UserId) && sr.Direction == SwipeDirection.Like)
            .GroupBy(sr => sr.RecipeId)
            .Where(g => g.Select(sr => sr.UserId).Distinct().Count() == userCount)
            .Select(g => g.Key)
            .ToListAsync();
    }

    public async Task<bool> CheckForNewMatch(IEnumerable<string> userIds, Guid recipeId)
    {
        var userIdList = userIds.ToList();
        var expectedUserCount = userIdList.Count;

        // Guard clause: Hvis der ikke er sendt nogen brugere med, kan der ikke være et match
        if (expectedUserCount == 0) return false;

        var actualUserCount = await _context.Set<Like>()
            .Where(sr => sr.RecipeId == recipeId 
                         && userIdList.Contains(sr.UserId) 
                         && sr.Direction == SwipeDirection.Like)
            .Select(sr => sr.UserId)
            .Distinct()
            .CountAsync();

        return actualUserCount == expectedUserCount;
    } 

    public async Task RecordMatchAsync(Match match)
    {
        await _context.Set<Match>().AddAsync(match);
    }
    
    public async Task DeleteMatchesByGroupIdAsync(Guid groupId)
    {
        var matches = _context.Set<Match>().Where(m => m.GroupId == groupId);
        _context.Set<Match>().RemoveRange(matches);
    }
}