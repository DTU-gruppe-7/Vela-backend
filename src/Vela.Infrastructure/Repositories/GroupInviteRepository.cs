using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class GroupInviteRepository(AppDbContext context) : Repository<GroupInvite>(context), IGroupInviteRepository
{
    public override Task<GroupInvite?> GetByUuidAsync(Guid uuid)
    {
        throw new NotSupportedException("GroupInvite uses composite key (UserId, GroupId). User GetGroupInviteAsync instead.");
    }

    public override Task DeleteAsync(Guid uuid)
    {
        throw new NotSupportedException("GroupInvite uses composite key (UserId, GroupId). User DeleteInviteAsync instead.");
    }

    public async Task<GroupInvite?> GetGroupInviteAsync(string userId, Guid groupId)
    {
        return await _context.GroupInvites
            .FirstOrDefaultAsync(gi => gi.GroupId == groupId && gi.UserId == userId);
    }

    public async Task<IEnumerable<GroupInvite?>> GetInvitesByUserIdAsync(string userId)
    {
        return await _context.GroupInvites
            .Where(gi => gi.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<GroupInvite?>> GetInvitesByGroupIdAsync(Guid groupId)
    {
        return await _context.GroupInvites
            .Where(gi => gi.GroupId == groupId)
            .ToListAsync();
    }

    public Task DeleteInviteAsync(GroupInvite groupInvite)
    {
        _context.GroupInvites.Remove(groupInvite);
        return Task.CompletedTask;
    }
}