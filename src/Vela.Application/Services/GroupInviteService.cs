using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class GroupInviteService(
    IGroupInviteRepository groupInviteRepository,
    IGroupRepository groupRepository,
    IGroupService groupService) : IGroupInviteService
{
    private readonly IGroupInviteRepository _groupInviteRepository = groupInviteRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IGroupService _groupService = groupService;

    public async Task<Result> SendInviteAsync(string userId, Guid groupId)
    {
        var group = await _groupRepository.GetByUuidAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        var invite = new GroupInvite
        {
            GroupId = groupId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _groupInviteRepository.AddAsync(invite);
        await _groupInviteRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AcceptInviteAsync(string userId, Guid groupId)
    {
        var group = await _groupRepository.GetByUuidAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");
        
        var invite = await _groupInviteRepository.GetGroupInviteAsync(userId, groupId);
        if (invite == null)
            return Result.Fail($"Invite for user {userId} in group {groupId} not found");
        
        var result = await _groupService.AddMemberAsync(groupId, userId); 
        
        if (!result.Success)
            return Result.Fail("Failed to add member");
        
        await _groupInviteRepository.DeleteInviteAsync(invite);

        await _groupInviteRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeclineInviteAsync(string userId, Guid groupId)
    {
        var invite = await _groupInviteRepository.GetGroupInviteAsync(userId, groupId);
        if (invite == null)
            return Result.Fail($"Invite for user {userId} in group {groupId} not found");
        
        await _groupInviteRepository.DeleteInviteAsync(invite);
        
        await _groupInviteRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByUserIdAsync(string userId)
    {
        var invites = await _groupInviteRepository.GetInvitesByUserIdAsync(userId);
        return Result<IEnumerable<GroupInviteDto>>.Ok(invites.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByGroupIdAsync(Guid groupId)
    {
        var invites = await _groupInviteRepository.GetInvitesByGroupIdAsync(groupId);
        return Result<IEnumerable<GroupInviteDto>>.Ok(invites.Select(MapToDto));
    }

    private GroupInviteDto MapToDto(GroupInvite invite)
    {
        return new GroupInviteDto
        {
            GroupId = invite.GroupId,
            UserId = invite.UserId,
            CreatedAt = invite.CreatedAt,
            UpdatedAt = invite.UpdatedAt
        };
    }
}