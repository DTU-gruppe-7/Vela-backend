using Vela.Application.Common;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;

namespace Vela.Application.Services;

public class UserOnboardingService(
	IAuthService authService,
	IShoppingListService shoppingListService,
	IMealPlanService mealPlanService) : IUserOnboardingService
{
	private readonly IAuthService _authService = authService;
	private readonly IShoppingListService _shoppingListService = shoppingListService;
	private readonly IMealPlanService _mealPlanService = mealPlanService;

	public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto requestDto)
	{
		var authResult = await _authService.RegisterAsync(requestDto);
		if (!authResult.Success)
			return authResult;

		var userId = authResult.Data!.UserId;

		var shoppingListResult = await _shoppingListService.CreateShoppingListAsync(userId, null, "Min indkøbsliste");
		if (!shoppingListResult.Success)
		{
			await _authService.DeleteUserAsync(userId);
			return Result<AuthResponseDto>.Fail(shoppingListResult.ErrorMessage!);
		}

		var mealPlanResult = await _mealPlanService.CreateMealPlanAsync(userId, null, "Min madplan");
		if (!mealPlanResult.Success)
		{
			await _authService.DeleteUserAsync(userId);
			return Result<AuthResponseDto>.Fail(mealPlanResult.ErrorMessage!);
		}

		return authResult;
	}
}
