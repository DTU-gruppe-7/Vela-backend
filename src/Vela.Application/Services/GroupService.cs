using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities.ShoppingList;
using Vela.Domain.Entities.Group;
using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Entities;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IMealPlanRepository mealPlanRepository,
    IShoppingListRepository shoppingListRepository,
    ILikeRepository likeRepository,
    ILikeService likeService,
    IUserRepository userRepository,
    IGroupAuthorizationService authorizationService) : IGroupService
{
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly ILikeRepository _likeRepository = likeRepository;
    private readonly ILikeService _likeService = likeService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IGroupAuthorizationService _authorizationService = authorizationService;

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

        var profiles = await _userRepository.GetUserProfilesByIdsAsync(group.Members.Select(m => m.UserId));
        return Result<GroupDto>.Ok(MapToDto(group, profiles));
    }

    public async Task<Result<GroupDto>> CreateGroupWithResourcesAsync(string userId, CreateGroupRequest request)
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

        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            Name = $"{request.Name} Meal Plan",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _mealPlanRepository.AddAsync(mealPlan);

        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            Name = $"{request.Name} Shopping List",
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _shoppingListRepository.AddAsync(shoppingList);

        await _groupRepository.SaveChangesAsync();

        await _likeService.RecalculateGroupMatchesAsync(group);

        var profiles = await _userRepository.GetUserProfilesByIdsAsync(group.Members.Select(m => m.UserId));
        return Result<GroupDto>.Ok(MapToDto(group, profiles));
    }

    public async Task<Result<GroupDto>> GetGroupAsync(Guid groupId, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result<GroupDto>.Fail($"Group with ID {groupId} not found");

        var authResult = _authorizationService.AuthorizeMembership(group, callerUserId);
        if (!authResult.Success)
            return Result<GroupDto>.Fail(authResult.ErrorMessage!);

        var profiles = await _userRepository.GetUserProfilesByIdsAsync(group.Members.Select(m => m.UserId));
        return Result<GroupDto>.Ok(MapToDto(group, profiles));
    }

    public async Task<Result<IEnumerable<GroupDto>>> GetGroupsByUserIdAsync(string userId)
    {
        var groups = await _groupRepository.GetGroupsByUserIdAsync(userId);
        var groupList = groups.Where(g => g != null).Select(g => g!).ToList();
        var allUserIds = groupList.SelectMany(g => g.Members.Select(m => m.UserId)).Distinct();
        var profiles = await _userRepository.GetUserProfilesByIdsAsync(allUserIds);
        return Result<IEnumerable<GroupDto>>.Ok(groupList.Select(g => MapToDto(g, profiles)));
    }

    public async Task<Result> DeleteGroupAsync(Guid groupId, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        var authResult = _authorizationService.AuthorizeDeleteGroup(group, callerUserId);
        if (!authResult.Success)
            return authResult;

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

    public async Task<Result> RemoveMemberAsync(Guid groupId, string targetUserId, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        var authResult = _authorizationService.AuthorizeRemoveMember(group, callerUserId, targetUserId);
        if (!authResult.Success)
            return authResult;

        var target = group.Members.First(m => m.UserId == targetUserId);
        group.Members.Remove(target);
        await _groupRepository.SaveChangesAsync();
        await _likeService.RecalculateGroupMatchesAsync(group);
        return Result.Ok();
    }

    public async Task<Result<IEnumerable<MatchDto>>> GetMatchesAsync(Guid groupId, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result<IEnumerable<MatchDto>>.Fail($"Group with ID {groupId} not found");

        var authResult = _authorizationService.AuthorizeMembership(group, callerUserId);
        if (!authResult.Success)
            return Result<IEnumerable<MatchDto>>.Fail(authResult.ErrorMessage!);

        var matches = await _groupRepository.GetMatchesByGroupIdAsync(groupId);
        return Result<IEnumerable<MatchDto>>.Ok(matches.Select(MapMatchToDto));
    }

    public async Task<Result> ChangeMemberRoleAsync(Guid groupId, string targetUserId, GroupRole newRole, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail("Group not found");

        var authResult = _authorizationService.AuthorizeChangeMemberRole(group, callerUserId, targetUserId, newRole);
        if (!authResult.Success)
            return authResult;

        var target = group.Members.First(m => m.UserId == targetUserId);
        target.Role = newRole;

        await _groupRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> LeaveGroupAsync(Guid groupId, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail("Group not found");

        var authResult = _authorizationService.AuthorizeLeaveGroup(group, callerUserId);
        if (!authResult.Success)
            return authResult;

        var caller = group.Members.First(m => m.UserId == callerUserId);
        group.Members.Remove(caller);
        await _groupRepository.SaveChangesAsync();
        await _likeService.RecalculateGroupMatchesAsync(group);
        return Result.Ok();
    }

    public async Task<Result> TransferOwnershipAsync(Guid groupId, string newOwnerUserId, string callerUserId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null)
            return Result.Fail("Group not found");

        var authResult = _authorizationService.AuthorizeTransferOwnership(group, callerUserId, newOwnerUserId);
        if (!authResult.Success)
            return authResult;

        var caller = group.Members.First(m => m.UserId == callerUserId);
        var newOwner = group.Members.First(m => m.UserId == newOwnerUserId);

        caller.Role = GroupRole.Administrator;
        newOwner.Role = GroupRole.Owner;

        await _groupRepository.SaveChangesAsync();
        return Result.Ok();
    }

    private GroupDto MapToDto(Group group, IReadOnlyDictionary<string, (string FirstName, string LastName, string Email)> profiles)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Status = group.Status,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Members = group.Members.Select(m => MapMemberToDto(m, profiles)).ToList()
        };
    }

    private GroupMemberDto MapMemberToDto(GroupMember member, IReadOnlyDictionary<string, (string FirstName, string LastName, string Email)> profiles)
    {
        profiles.TryGetValue(member.UserId, out var profile);
        return new GroupMemberDto
        {
            GroupId = member.GroupId,
            UserId = member.UserId,
            FirstName = profile.FirstName ?? string.Empty,
            LastName = profile.LastName ?? string.Empty,
            Email = profile.Email ?? string.Empty,
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