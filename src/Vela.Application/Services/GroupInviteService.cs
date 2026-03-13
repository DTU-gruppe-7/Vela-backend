using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class GroupInviteService(
    IGroupInviteRepository groupInviteRepository,
    IGroupRepository groupRepository) : IGroupInviteService
{
    private readonly IGroupInviteRepository _groupInviteRepository = groupInviteRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;

    public async Task<Result> SendInviteAsync(Guid groupId, string userId)
    {
        var group = await _groupRepository.GetByUuidAsync(groupId);
        if (group == null)
            return Result.Fail($"Group with ID {groupId} not found");

        var invite = new GroupInvite
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            UserId = userId,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _groupInviteRepository.AddAsync(invite);
        await _groupInviteRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AcceptInviteAsync(Guid inviteId)
    {
        var invite = await _groupInviteRepository.GetByUuidAsync(inviteId);
        if (invite == null)
            return Result.Fail($"Invite with ID {inviteId} not found");

        await _groupInviteRepository.UpdateStatusAsync(inviteId, "Accepted");
        await _groupInviteRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeclineInviteAsync(Guid inviteId)
    {
        var invite = await _groupInviteRepository.GetByUuidAsync(inviteId);
        if (invite == null)
            return Result.Fail($"Invite with ID {inviteId} not found");

        await _groupInviteRepository.UpdateStatusAsync(inviteId, "Declined");
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
            Id = invite.Id,
            GroupId = invite.GroupId,
            UserId = invite.UserId,
            Status = invite.Status,
            CreatedAt = invite.CreatedAt,
            UpdatedAt = invite.UpdatedAt
        };
    }
}