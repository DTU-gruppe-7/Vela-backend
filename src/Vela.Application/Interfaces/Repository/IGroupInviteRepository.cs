using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IGroupInviteRepository : IRepository<GroupInvite>
{
    Task<IEnumerable<GroupInvite>> GetInvitesByUserIdAsync(string userId);
    Task<IEnumerable<GroupInvite>> GetInvitesByGroupIdAsync(Guid groupId);
    Task UpdateStatusAsync(Guid inviteId, string status);
}