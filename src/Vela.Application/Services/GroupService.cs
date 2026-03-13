using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IMealPlanRepository mealPlanRepository,
    IShoppingListRepository shoppingListRepository) : IGroupService
{
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;

    public async Task<Result<GroupDto>> CreateGroupAsync(string userId, CreateGroupRequest request)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = group.Id,
            Name = $"{request.Name}'s madplan",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = group.Id,
            Name = $"{request.Name}'s indkøbsliste",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var owner = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            Role = "Owner",
            JoinedAt = DateTimeOffset.UtcNow
        };

        await _groupRepository.AddAsync(group);
        await _groupRepository.AddMemberAsync(owner);
        await _mealPlanRepository.AddAsync(mealPlan);
        await _shoppingListRepository.AddAsync(shoppingList);
        await _groupRepository.SaveChangesAsync();

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

        await _groupRepository.DeleteAsync(groupId);
        await _groupRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AddMemberAsync(Guid groupId, AddMemberRequest request)
    {
        var group = await _groupRepository.GetByUuidAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        var existing = await _groupRepository.GetMemberAsync(groupId, request.UserId);
        if (existing != null)
            return Result.Fail("User is already a member of this group");

        var member = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            UserId = request.UserId,
            Role = request.Role,
            JoinedAt = DateTimeOffset.UtcNow
        };

        await _groupRepository.AddMemberAsync(member);
        await _groupRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> RemoveMemberAsync(Guid groupId, string userId)
    {
        var member = await _groupRepository.GetMemberAsync(groupId, userId);
        if (member == null)
            return Result.Fail("Member not found in this group");

        await _groupRepository.RemoveMemberAsync(groupId, userId);
        await _groupRepository.SaveChangesAsync();
        return Result.Ok();
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
            Id = member.Id,
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
            Id = match.Id,
            GroupId = match.GroupId,
            RecipeId = match.RecipeId,
            MatchedAt = match.MatchedAt
        };
    }
}