using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class GroupRepository(AppDbContext context) : Repository<Group>(context), IGroupRepository
{
    public async Task<Group?> GetGroupWithMembersAsync(Guid groupId)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<IEnumerable<Group?>> GetGroupsByUserIdAsync(string userId)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Match?>> GetMatchesByGroupIdAsync(Guid groupId)
    {
        return await  _context.Matches
            .Where(g => g.GroupId == groupId)
            .ToListAsync();
    }
}