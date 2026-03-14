using System.Runtime.InteropServices.ComTypes;
using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IGroupInviteRepository : IRepository<GroupInvite>
{
    Task<GroupInvite?> GetGroupInviteAsync(string userId, Guid groupId);
    Task<IEnumerable<GroupInvite?>> GetInvitesByUserIdAsync(string userId);
    Task<IEnumerable<GroupInvite?>> GetInvitesByGroupIdAsync(Guid groupId);
    Task DeleteInviteAsync(GroupInvite groupInvite);
}