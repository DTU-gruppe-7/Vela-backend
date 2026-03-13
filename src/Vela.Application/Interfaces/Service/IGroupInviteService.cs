using Vela.Application.Common;
using Vela.Application.DTOs.Group;

namespace Vela.Application.Interfaces.Service;

public interface IGroupInviteService
{
    Task<Result> SendInviteAsync(Guid groupId, string userId);
    Task<Result> AcceptInviteAsync(Guid inviteId);
    Task<Result> DeclineInviteAsync(Guid inviteId);
    Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByUserIdAsync(string userId);
    Task<Result<IEnumerable<GroupInviteDto>>> GetInvitesByGroupIdAsync(Guid groupId);
}