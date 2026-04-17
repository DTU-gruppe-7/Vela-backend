namespace Vela.Application.Interfaces.Repository;

public interface IUserRepository
{
	Task<IReadOnlyDictionary<string, (string FirstName, string LastName, string Email)>> GetUserProfilesByIdsAsync(IEnumerable<string> userIds);
}
