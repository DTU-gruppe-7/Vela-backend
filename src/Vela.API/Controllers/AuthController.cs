using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vela.Application.Configuration;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[ApiVersion("1.0")]
public class AuthController(
	IAuthService authService,
	IUserOnboardingService userOnboardingService,
	IUserProfileService userProfileService,
	IWebHostEnvironment environment,
	IOptions<JwtSettings> jwtSettings) : BaseApiController
{
	private const string RefreshTokenCookieName = "refreshToken";
	private const string RefreshTokenCookiePath = "/api/v1/Auth";

	private readonly IAuthService _authService = authService;
	private readonly IUserOnboardingService _userOnboardingService = userOnboardingService;
	private readonly IUserProfileService _userProfileService = userProfileService;
	private readonly IWebHostEnvironment _environment = environment;
	private readonly JwtSettings _jwtSettings = jwtSettings.Value;

	[HttpPost("register")]
	public async Task<ActionResult<AuthLoginResponseDto>> Register([FromBody] RegisterRequestDto registerRequestDto)
	{
		var result = await _userOnboardingService.RegisterAsync(registerRequestDto);
		if (!result.Success)
			return BadRequest(new { message = result.ErrorMessage });

		AppendRefreshTokenCookie(result.Data!.RefreshToken);
		return Ok(MapLoginResponse(result.Data!));
	}

	[HttpPost("login")]
	public async Task<ActionResult<AuthLoginResponseDto>> Login([FromBody] LoginRequestDto loginRequestDto)
	{
		var result = await _authService.LoginAsync(loginRequestDto);
		if (!result.Success)
			return Unauthorized(result.ErrorMessage);

		AppendRefreshTokenCookie(result.Data!.RefreshToken);
		return Ok(MapLoginResponse(result.Data!));
	}

	[HttpPost("refresh")]
	public async Task<ActionResult<AuthLoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
	{
		var refreshToken = Request.Cookies[RefreshTokenCookieName];
		if (string.IsNullOrWhiteSpace(refreshToken))
			return Unauthorized("Missing refresh token cookie.");

		var result = await _authService.RefreshTokenAsync(refreshTokenRequestDto.AccessToken, refreshToken);
		if (!result.Success)
			return Unauthorized(result.ErrorMessage);

		AppendRefreshTokenCookie(result.Data!.RefreshToken);
		return Ok(MapLoginResponse(result.Data!));
	}

	[Authorize]
	[HttpPost("logout")]
	public async Task<IActionResult> Logout()
	{
		var userId = GetCurrentUserId();
		var result = await _authService.LogoutAsync(userId);

		if (!result.Success)
			return BadRequest(result.ErrorMessage);

		DeleteRefreshTokenCookie();
		return NoContent();
	}

	[Authorize]
	[HttpGet("preferences")]
	public async Task<IActionResult> GetDietaryPreferences()
	{
		var userId = GetCurrentUserId();
		var result = await _userProfileService.GetDietaryPreferencesAsync(userId);

		if (!result.Success)
			return NotFound(new { message = result.ErrorMessage });

		return Ok(result.Data);
	}

	[Authorize]
	[HttpPatch("preferences")]
	public async Task<IActionResult> UpdateDietaryPreferences([FromBody] UpdateUserDietaryPreferencesRequestDto requestDto)
	{
		var userId = GetCurrentUserId();
		var result = await _userProfileService.UpdateDietaryPreferencesAsync(userId, requestDto);

		if (!result.Success)
			return BadRequest(new { message = result.ErrorMessage });

		return Ok(result.Data);
	}

	private void AppendRefreshTokenCookie(string refreshToken)
	{
		Response.Cookies.Append(
			RefreshTokenCookieName,
			refreshToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = !_environment.IsDevelopment(),
				SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
				Path = RefreshTokenCookiePath,
				Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenValidityInDays),
				IsEssential = true
			});
	}

	private void DeleteRefreshTokenCookie()
	{
		Response.Cookies.Delete(
			RefreshTokenCookieName,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = !_environment.IsDevelopment(),
				SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
				Path = RefreshTokenCookiePath
			});
	}

	private static AuthLoginResponseDto MapLoginResponse(AuthResponseDto authResponseDto)
	{
		return new AuthLoginResponseDto(
			authResponseDto.Token,
			new AuthUserDto(
				authResponseDto.UserId,
				authResponseDto.Email,
				authResponseDto.FirstName,
				authResponseDto.LastName,
				authResponseDto.ProfilePictureUrl,
				authResponseDto.DateOfBirth));
	}
}
