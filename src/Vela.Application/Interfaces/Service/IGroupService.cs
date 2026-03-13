using Vela.Application.Common;
using Vela.Application.DTOs.Group;

namespace Vela.Application.Interfaces.Service;

public interface IGroupService
{
    Task<Result<GroupDto>> CreateGroupAsync(string userId, CreateGroupRequest request);
    Task<Result<GroupDto>> GetGroupAsync(Guid groupId);
    Task<Result<IEnumerable<GroupDto>>> GetGroupsByUserIdAsync(string userId);
    Task<Result> DeleteGroupAsync(Guid groupId);
    Task<Result> AddMemberAsync(Guid groupId, AddMemberRequest request);
    Task<Result> RemoveMemberAsync(Guid groupId, string userId);
    Task<Result<IEnumerable<MatchDto>>> GetMatchesAsync(Guid groupId);
}