using Vela.Application.Common;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class GroupAuthorizationService : IGroupAuthorizationService
{
    public Result AuthorizeDeleteGroup(Group group, string callerUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null || caller.Role != GroupRole.Owner)
            return Result.Fail("Only the owner can delete a group");

        if (group.Members.Count > 1)
            return Result.Fail("Owner must be the only member left to delete the group");

        return Result.Ok();
    }

    public Result AuthorizeRemoveMember(Group group, string callerUserId, string targetUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null)
            return Result.Fail("You are not a member of this group");

        var target = FindMember(group, targetUserId);
        if (target == null)
            return Result.Fail($"No user with ID {targetUserId} found in group with ID {group.Id}");

        if (caller.Role == GroupRole.Owner)
        {
            if (targetUserId == callerUserId)
                return Result.Fail("Owner cannot remove themselves. Use leave or transfer ownership.");
        }
        else if (caller.Role == GroupRole.Administrator)
        {
            if (target.Role != GroupRole.Member)
                return Result.Fail("Administrators can only remove members, not other administrators or the owner");
        }
        else
        {
            return Result.Fail("Members cannot remove other users");
        }

        return Result.Ok();
    }

    public Result AuthorizeChangeMemberRole(Group group, string callerUserId, string targetUserId, GroupRole newRole)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null)
            return Result.Fail("You are not a member of this group");

        var target = FindMember(group, targetUserId);
        if (target == null)
            return Result.Fail("Target user is not a member of this group");

        if (newRole == GroupRole.Owner)
            return Result.Fail("Use the transfer-ownership endpoint to make someone owner");

        if (caller.Role == GroupRole.Owner)
            return Result.Ok();

        if (caller.Role == GroupRole.Administrator)
        {
            if (target.Role != GroupRole.Member || newRole != GroupRole.Administrator)
                return Result.Fail("Administrators can only promote members to administrator");
            return Result.Ok();
        }

        return Result.Fail("Members cannot change roles");
    }

    public Result AuthorizeLeaveGroup(Group group, string callerUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null)
            return Result.Fail("You are not a member of this group");

        if (caller.Role == GroupRole.Owner)
            return Result.Fail("Owner must transfer ownership before leaving the group");

        return Result.Ok();
    }

    public Result AuthorizeTransferOwnership(Group group, string callerUserId, string newOwnerUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null || caller.Role != GroupRole.Owner)
            return Result.Fail("Only the owner can transfer ownership");

        var newOwner = FindMember(group, newOwnerUserId);
        if (newOwner == null)
            return Result.Fail("Target user is not a member of this group");

        if (newOwnerUserId == callerUserId)
            return Result.Fail("You are already the owner");

        return Result.Ok();
    }

    public Result AuthorizeSendInvite(Group group, string callerUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null)
            return Result.Fail("You are not a member of this group");

        if (caller.Role == GroupRole.Member)
            return Result.Fail("Only owners and administrators can send invites");

        return Result.Ok();
    }

    public Result AuthorizeViewInvites(Group group, string callerUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null)
            return Result.Fail("You are not a member of this group");

        if (caller.Role == GroupRole.Member)
            return Result.Fail("Only owners and administrators can view invites");

        return Result.Ok();
    }

    public Result AuthorizeMembership(Group group, string callerUserId)
    {
        var caller = FindMember(group, callerUserId);
        if (caller == null)
            return Result.Fail("You are not a member of this group");

        return Result.Ok();
    }

    private static GroupMember? FindMember(Group group, string userId)
    {
        return group.Members.FirstOrDefault(m => m.UserId == userId);
    }
}