

using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class GroupInviteRepository(AppDbContext context) : Repository<GroupInvite>(context), IGroupInviteRepository
{
    public Task<IEnumerable<GroupInvite>> GetInvitesByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroupInvite>> GetInvitesByGroupIdAsync(Guid groupId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateStatusAsync(Guid inviteId, string status)
    {
        throw new NotImplementedException();
    }
}