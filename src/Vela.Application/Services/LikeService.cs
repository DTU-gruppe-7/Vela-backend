using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Domain.Entities;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class LikeService(
    IRecipeRepository recipeRepository,
    ILikeRepository likeRepository,
    IGroupRepository groupRepository,
    INotificationDispatcher notificationDispatcher) : ILikeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository;
    private readonly ILikeRepository _likeRepository  = likeRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly INotificationDispatcher _notificationDispatcher = notificationDispatcher;

    public async Task<Result> RecordSwipeAsync(string userId, SwipeDto swipeDto)
    {
        var recipe = await _recipeRepository.GetByUuidAsync(swipeDto.RecipeId);
        if (recipe == null)
        {
            return Result.Fail("Recipe not found");
        }

        var alreadySwiped = await _likeRepository.HasUserSwipedOnRecipeAsync(userId, swipeDto.RecipeId);
        if (alreadySwiped)
            return Result.Ok();

        var swipe = new Like
        {
            LikeId = Guid.NewGuid(),
            UserId = userId,
            RecipeId = swipeDto.RecipeId,
            Direction = swipeDto.Direction,
            SwipedAt = DateTimeOffset.UtcNow,
        };
        await _likeRepository.AddAsync(swipe);
        await _likeRepository.SaveChangesAsync();
        
        var groups = await _groupRepository.GetGroupsByUserIdAsync(userId);
        await CheckForGroupMatchesAsync(groups, swipeDto.RecipeId);

        return Result.Ok();
    }

    public async Task RecordGroupMatch(Guid groupId, Guid recipeId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            Result.Fail("Group not found");
        
        var match = new Match
        {
            GroupId = groupId,
            RecipeId = recipeId,
            MatchedAt = DateTimeOffset.UtcNow
        };
        await _likeRepository.RecordMatchAsync(match);
        await _likeRepository.SaveChangesAsync();

        foreach (var userId in group.Members.Select(m => m.UserId))
        {
            await _notificationDispatcher.DispatchAsync(
                userId,
                "Nyt match!",
                $"Der er kommet et nyt match i {group.Name} gruppen",
                NotificationType.NewMatch,
                recipeId);
        }
    }
    
    public async Task<IEnumerable<RecipeSummaryDto>> GetLikedRecipesByUserIdAsync(string userId)
    {
        var likedRecipes = await _likeRepository.GetLikedRecipesByUserIdAsync(userId);
        return likedRecipes.Select(r => new RecipeSummaryDto
        {
            Id = r.Id,
            Name = r.Name,
            Category = r.Category,
            ThumbnailUrl = r.ThumbnailUrl,
            WorkTime = r.WorkTime,
            TotalTime = r.TotalTime,
            KeywordsJson = r.KeywordsJson,
        });
    }
    
    public async Task RecalculateGroupMatchesAsync(Group group)
    {
        await _likeRepository.DeleteMatchesByGroupIdAsync(group.Id);
        await _likeRepository.SaveChangesAsync();
        var commonLikes = await _likeRepository.GetCommonLikedRecipeIdsAsync(group.Members.Select(m => m.UserId));

        foreach (var recipeId in commonLikes)
        {
            var match = new Match
            {
                GroupId = group.Id,
                RecipeId = recipeId,
                MatchedAt = DateTimeOffset.UtcNow
            };
            await _likeRepository.RecordMatchAsync(match);
        }
        await _likeRepository.SaveChangesAsync();
    }

    private async Task CheckForGroupMatchesAsync(IEnumerable<Group?> groups, Guid recipeId)
    {
        foreach (var group in groups)
        {
            if (group == null) continue;
            var userIds = group.Members.Select(m => m.UserId).ToList();
            var foundMatch = await _likeRepository.CheckForNewMatch(userIds, recipeId);

            if (foundMatch)
            {
                await RecordGroupMatch(group.Id, recipeId);
            }
        }
    }
}
