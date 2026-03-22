using Vela.Domain.Entities.Group;
using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IGroupRepository : IRepository<Group>
{
    Task<Group?> GetGroupWithMembersAsync(Guid groupId);
    Task<IEnumerable<Group?>> GetGroupsByUserIdAsync(string userId);
    Task<IEnumerable<Match?>> GetMatchesByGroupIdAsync(Guid groupId);
}