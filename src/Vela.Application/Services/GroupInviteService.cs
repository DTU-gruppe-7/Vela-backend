using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Domain.Entities.Group;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class GroupInviteService(
	IGroupInviteRepository groupInviteRepository,
	IGroupRepository groupRepository,
	IGroupService groupService,
	IGroupAuthorizationService authorizationService,
	INotificationDispatcher notificationDispatcher,
	IUserRepository userRepository) : IGroupInviteService
{
	private readonly IGroupInviteRepository _groupInviteRepository = groupInviteRepository;
	private readonly IGroupRepository _groupRepository = groupRepository;
	private readonly IGroupService _groupService = groupService;
	private readonly IGroupAuthorizationService _authorizationService = authorizationService;
	private readonly INotificationDispatcher _notificationDispatcher = notificationDispatcher;
	private readonly IUserRepository _userRepository = userRepository;

	public async Task<Result> SendInviteAsync(string email, Guid groupId, string callerUserId)
	{
		var userId = await _userRepository.FindUserIdByEmailAsync(email);
		if (userId == null)
			return Result.Fail("Der findes ingen bruger med denne e-mail adresse.");

		var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
		if (group == null)
			return Result.Fail($"Group with ID {groupId} not found");

		var authResult = _authorizationService.AuthorizeSendInvite(group, callerUserId);
		if (!authResult.Success)
			return authResult;

		var existingInvite = await _groupInviteRepository.GetGroupInviteAsync(userId, groupId);
		if (existingInvite != null)
			return Result.Fail("Der er allerede sendt en invitation til denne bruger for denne gruppe.");

		var invite = new GroupInvite
		{
			GroupId = groupId,
			UserId = userId,
			CreatedAt = DateTimeOffset.UtcNow
		};

		await _groupInviteRepository.AddAsync(invite);
		await _groupInviteRepository.SaveChangesAsync();

		await _notificationDispatcher.DispatchAsync(
			userId,
			"Ny gruppeinvitation",
			$"Du er inviteret til gruppen {group.Name}",
			NotificationType.GroupInvite,
			groupId);

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
		return Result<IEnumerable<GroupInviteDto>>.Ok(invites.Where(i => i != null).Select(i => MapToDto(i!)));
	}

	public async Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByGroupIdAsync(Guid groupId, string callerUserId)
	{
		var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
		if (group == null)
			return Result<IEnumerable<GroupInviteDto>>.Fail($"Group with ID {groupId} not found");

		var authResult = _authorizationService.AuthorizeViewInvites(group, callerUserId);
		if (!authResult.Success)
			return Result<IEnumerable<GroupInviteDto>>.Fail(authResult.ErrorMessage!);

		var invites = await _groupInviteRepository.GetInvitesByGroupIdAsync(groupId);
		return Result<IEnumerable<GroupInviteDto>>.Ok(invites.Where(i => i != null).Select(i => MapToDto(i!)));
	}

	private static GroupInviteDto MapToDto(GroupInvite invite)
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
