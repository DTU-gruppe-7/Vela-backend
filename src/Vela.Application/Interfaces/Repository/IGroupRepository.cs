using Vela.Domain.Entities;

namespace Vela.Application.Interfaces.Repository;

public interface IGroupRepository : IRepository<Group>
{
    Task<Group?> GetGroupWithMembersAsync(Guid groupId);
    Task<IEnumerable<Group>> GetGroupsByUserIdAsync(string userId);
    Task AddMemberAsync(GroupMember member);
    Task RemoveMemberAsync(Guid groupId, string userId);
    Task<GroupMember?> GetMemberAsync(Guid groupId, string userId);
    Task<IEnumerable<Match>> GetMatchesByGroupIdAsync(Guid groupId);
}