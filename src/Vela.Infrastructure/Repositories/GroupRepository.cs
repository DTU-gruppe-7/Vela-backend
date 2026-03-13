using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class GroupRepository(AppDbContext context) : Repository<Group>(context), IGroupRepository
{
    public Task<Group?> GetGroupWithMembersAsync(Guid groupId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Group>> GetGroupsByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<GroupMember?> GetMemberAsync(Guid groupId, string userId)
    {
        throw new NotImplementedException();
    }
    
    public Task<IEnumerable<Match>> GetMatchesByGroupIdAsync(Guid groupId)
    {
        throw new NotImplementedException();
    }
}