using Vela.Application.Common;
using Vela.Domain.Entities;
using Vela.Domain.Enums;

namespace Vela.Application.Interfaces.Service;

public interface IGroupAuthorizationService
{
    Result AuthorizeDeleteGroup(Group group, string callerUserId);
    Result AuthorizeRemoveMember(Group group, string callerUserId, string targetUserId);
    Result AuthorizeChangeMemberRole(Group group, string callerUserId, string targetUserId, GroupRole newRole);
    Result AuthorizeLeaveGroup(Group group, string callerUserId);
    Result AuthorizeTransferOwnership(Group group, string callerUserId, string newOwnerUserId);
    Result AuthorizeSendInvite(Group group, string callerUserId);
    Result AuthorizeViewInvites(Group group, string callerUserId);
    Result AuthorizeMembership(Group group, string callerUserId);
}