using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IShoppingListService shoppingListService, IMealPlanService mealPlanService) : BaseApiController
{
    private readonly IAuthService _authService =  authService;
    private readonly IShoppingListService _shoppingListService = shoppingListService;
    private readonly IMealPlanService _mealPlanService = mealPlanService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
    {
        var result = await _authService.RegisterAsync(registerRequestDto);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        var userId = GetCurrentUserId();
        var shoppingListResult = await _shoppingListService.CreateShoppingListAsync(userId, null, "Min indkøbsliste");

        if (!shoppingListResult.Success)
        {
            await _authService.DeleteUserAsync(userId);
            return BadRequest(shoppingListResult.ErrorMessage);
        }
        
        var mealPlanResult = await _mealPlanService.CreateMealPlanAsync(userId, null, "Min madplan");
        if (!mealPlanResult.Success)
        {
            await _authService.DeleteUserAsync(userId);
            return BadRequest(mealPlanResult.ErrorMessage);
        }
        
        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var result = await _authService.LoginAsync(loginRequestDto);
        if (!result.Success)
        {
            return Unauthorized(result.ErrorMessage); // 401 Unauthorized er bedst til mislykket login
        }
        return Ok(result.Data);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenRequestDto);
        if (!result.Success)
        {
            return Unauthorized(result.ErrorMessage); 
        }

        return Ok(result.Data);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        var result = await _authService.LogoutAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        
        return NoContent();
    }
} 