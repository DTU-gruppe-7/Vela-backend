using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
	public async Task<IReadOnlyDictionary<string, (string FirstName, string LastName, string Email)>> GetUserProfilesByIdsAsync(IEnumerable<string> userIds)
	{
		var ids = userIds.ToList();
		return await context.Users
			.Where(u => ids.Contains(u.Id))
			.ToDictionaryAsync(
				u => u.Id,
				u => (u.FirstName, u.LastName, u.Email ?? string.Empty));
	}
}
