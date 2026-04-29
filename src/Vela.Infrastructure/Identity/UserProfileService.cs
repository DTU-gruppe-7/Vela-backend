using Microsoft.AspNetCore.Identity;
using Vela.Application.Common;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;

namespace Vela.Infrastructure.Identity;

public class UserProfileService(UserManager<AppUser> userManager) : IUserProfileService
{
	private readonly UserManager<AppUser> _userManager = userManager;

	public async Task<Result<UserDietaryPreferencesDto>> GetDietaryPreferencesAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
			return Result<UserDietaryPreferencesDto>.Fail("User not found");

		return Result<UserDietaryPreferencesDto>.Ok(new UserDietaryPreferencesDto
		{
			AvoidGluten = user.AvoidGluten,
			AvoidLactose = user.AvoidLactose,
			AvoidNuts = user.AvoidNuts,
			IsVegan = user.IsVegan,
		});
	}

	public async Task<Result<UserDietaryPreferencesDto>> UpdateDietaryPreferencesAsync(string userId, UpdateUserDietaryPreferencesRequestDto requestDto)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
			return Result<UserDietaryPreferencesDto>.Fail("User not found");

		if (requestDto.AvoidGluten.HasValue)
			user.AvoidGluten = requestDto.AvoidGluten.Value;

		if (requestDto.AvoidLactose.HasValue)
			user.AvoidLactose = requestDto.AvoidLactose.Value;

		if (requestDto.AvoidNuts.HasValue)
			user.AvoidNuts = requestDto.AvoidNuts.Value;

		if (requestDto.IsVegan.HasValue)
			user.IsVegan = requestDto.IsVegan.Value;

		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			return Result<UserDietaryPreferencesDto>.Fail($"Could not update dietary preferences: {errors}");
		}

		return Result<UserDietaryPreferencesDto>.Ok(new UserDietaryPreferencesDto
		{
			AvoidGluten = user.AvoidGluten,
			AvoidLactose = user.AvoidLactose,
			AvoidNuts = user.AvoidNuts,
			IsVegan = user.IsVegan,
		});
	}
}
