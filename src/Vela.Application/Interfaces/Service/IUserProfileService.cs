using Vela.Application.Common;
using Vela.Application.DTOs.Auth;

namespace Vela.Application.Interfaces.Service;

public interface IUserProfileService
{
	Task<Result<UserDietaryPreferencesDto>> GetDietaryPreferencesAsync(string userId);
	Task<Result<UserDietaryPreferencesDto>> UpdateDietaryPreferencesAsync(string userId, UpdateUserDietaryPreferencesRequestDto requestDto);
}
