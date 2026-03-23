using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Domain.Enums;

namespace Vela.Application.Interfaces.Service;

public interface IGroupService
{
    Task<Result<GroupDto>> CreateGroupAsync(string userId, CreateGroupRequest request);
    Task<Result<GroupDto>> CreateGroupWithResourcesAsync(string userId, CreateGroupRequest request);
    Task<Result<GroupDto>> GetGroupAsync(Guid groupId, string callerUserId);
    Task<Result<IEnumerable<GroupDto>>> GetGroupsByUserIdAsync(string userId);
    Task<Result> DeleteGroupAsync(Guid groupId, string callerUserId);
    Task<Result> AddMemberAsync(Guid groupId, string userId);
    Task<Result> RemoveMemberAsync(Guid groupId, string userId, string callerUserId);
    Task<Result<IEnumerable<MatchDto>>> GetMatchesAsync(Guid groupId, string callerUserId);
    Task<Result> ChangeMemberRoleAsync(Guid groupId, string targetUserId, GroupRole newRole, string callerUserId);
    Task<Result> LeaveGroupAsync(Guid groupId, string callerUserId);
    Task<Result> TransferOwnershipAsync(Guid groupId, string newOwnerUserId, string callerUserId);
}