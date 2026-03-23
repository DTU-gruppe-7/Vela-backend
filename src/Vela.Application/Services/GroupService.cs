using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;
using Vela.Domain.Entities.Group;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IMealPlanRepository mealPlanRepository,
    IShoppingListRepository shoppingListRepository,
    ILikeRepository likeRepository,
    ILikeService likeService) : IGroupService
{
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly ILikeRepository _likeRepository = likeRepository;
    private readonly ILikeService _likeService = likeService;

    public async Task<Result<GroupDto>> CreateGroupAsync(string userId, CreateGroupRequest request)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var owner = new GroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Owner,
            JoinedAt = DateTimeOffset.UtcNow
        };

        group.Members.Add(owner);
        
        await _groupRepository.AddAsync(group);
        await _groupRepository.SaveChangesAsync();

        await _likeService.RecalculateGroupMatchesAsync(group);

        return Result<GroupDto>.Ok(MapToDto(group));
    }

    public async Task<Result<GroupDto>> GetGroupAsync(Guid groupId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result<GroupDto>.Fail($"Group with ID {groupId} not found");

        return Result<GroupDto>.Ok(MapToDto(group));
    }

    public async Task<Result<IEnumerable<GroupDto>>> GetGroupsByUserIdAsync(string userId)
    {
        var groups = await _groupRepository.GetGroupsByUserIdAsync(userId);
        return Result<IEnumerable<GroupDto>>.Ok(groups.Select(MapToDto));
    }

    public async Task<Result> DeleteGroupAsync(Guid groupId)
    {
        var group = await _groupRepository.GetByUuidAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        await _groupRepository.DeleteAsync(group);
        await _groupRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AddMemberAsync(Guid groupId, string userId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        var member = new GroupMember
        {
            GroupId = groupId,
            UserId = userId,
            Role = GroupRole.Member,
            JoinedAt = DateTimeOffset.UtcNow
        };

        group.Members.Add(member);

        await _groupRepository.SaveChangesAsync();
        
        await _likeService.RecalculateGroupMatchesAsync(group);
        
        return Result.Ok();
    }

    public async Task<Result> RemoveMemberAsync(Guid groupId, string userId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");
        foreach (var member in group.Members)
        {
            if (member.UserId == userId)
            {
                group.Members.Remove(member);
                await _groupRepository.SaveChangesAsync();
                await _likeService.RecalculateGroupMatchesAsync(group);
                return Result.Ok();
            }
        }

        return Result.Fail($"No user with ID {userId} not found in group with ID {groupId}");
    }

    public async Task<Result<IEnumerable<MatchDto>>> GetMatchesAsync(Guid groupId)
    {
        var matches = await _groupRepository.GetMatchesByGroupIdAsync(groupId);
        return Result<IEnumerable<MatchDto>>.Ok(matches.Select(MapMatchToDto));
    }

private GroupDto MapToDto(Group group)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Status = group.Status,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Members = group.Members.Select(MapMemberToDto).ToList()
        };
    }

    private GroupMemberDto MapMemberToDto(GroupMember member)
    {
        return new GroupMemberDto
        {
            GroupId = member.GroupId,
            UserId = member.UserId,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }

    private MatchDto MapMatchToDto(Match match)
    {
        return new MatchDto
        {
            GroupId = match.GroupId,
            RecipeId = match.RecipeId,
            MatchedAt = match.MatchedAt
        };
    }
}