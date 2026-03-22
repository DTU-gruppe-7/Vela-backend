using Vela.Application.Common;
using Vela.Application.DTOs.Group;

namespace Vela.Application.Interfaces.Service;

public interface IGroupInviteService
{
    Task<Result> SendInviteAsync(string userId, Guid groupId, string callerUserId);
    Task<Result> AcceptInviteAsync(string userId, Guid groupId);
    Task<Result> DeclineInviteAsync(string userId, Guid groupId);
    Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByUserIdAsync(string userId);
    Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByGroupIdAsync(Guid groupId, string callerUserId);
}