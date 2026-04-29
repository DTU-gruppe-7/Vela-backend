using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vela.Application.Common;
using Vela.Application.Configuration;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;

namespace Vela.Infrastructure.Identity;

public class AuthService(UserManager<AppUser> userManager, IOptions<JwtSettings> jwtOptions) : IAuthService
{
	private readonly UserManager<AppUser> _userManager = userManager;
	private readonly JwtSettings _jwt = jwtOptions.Value;

	public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto requestDto)
	{
		var existingUser = await _userManager.FindByEmailAsync(requestDto.Email);
		if (existingUser != null)
			return Result<AuthResponseDto>.Fail("Registration failed. Please check your details.");

		if (requestDto.DateOfBirth.HasValue)
		{
			var dob = requestDto.DateOfBirth.Value;
			if (dob >= DateOnly.FromDateTime(DateTime.UtcNow))
				return Result<AuthResponseDto>.Fail("Date of birth cannot be in the future.");
			if (dob < new DateOnly(1900, 1, 1))
				return Result<AuthResponseDto>.Fail("Date of birth cannot be before 1900-01-01.");
		}

		var newUser = new AppUser
		{
			UserName = requestDto.Email,
			Email = requestDto.Email,
			FirstName = requestDto.FirstName,
			LastName = requestDto.LastName,
			DateOfBirth = requestDto.DateOfBirth,
		};

		var result = await _userManager.CreateAsync(newUser, requestDto.Password);

		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			return Result<AuthResponseDto>.Fail($"Could not create user: {errors}");
		}

		return await GenerateUserTokenAsync(newUser);
	}

	public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto requestDto)
	{
		var user = await _userManager.FindByEmailAsync(requestDto.Email);

		if (user == null || !await _userManager.CheckPasswordAsync(user, requestDto.Password))
			return Result<AuthResponseDto>.Fail("Wrong username or password.");

		return await GenerateUserTokenAsync(user);
	}

	public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string token, string refreshToken)
	{
		ClaimsPrincipal? principal;
		try { principal = GetPrincipalFromExpiredToken(token); }
		catch (SecurityTokenException) { return Result<AuthResponseDto>.Fail("Invalid or expired token. Please login again."); }

		if (principal == null)
			return Result<AuthResponseDto>.Fail("Invalid token.");

		var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
		if (string.IsNullOrEmpty(email))
			return Result<AuthResponseDto>.Fail("Invalid token.");

		var user = await _userManager.FindByEmailAsync(email);

		if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
			return Result<AuthResponseDto>.Fail("Invalid or expired token. Please login again.");

		return await GenerateUserTokenAsync(user);
	}

	public async Task<Result> DeleteUserAsync(string userId)
	{
		try
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return Result.Fail("User not found");

			var result = await _userManager.DeleteAsync(user);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				return Result.Fail($"Could not delete user: {errors}");
			}
			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail($"An error occurred while deleting user: {ex.Message}");
		}
	}

	public async Task<Result<bool>> LogoutAsync(string userId)
	{
		try
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
				return Result<bool>.Fail("User not found");

			user.RefreshToken = null;
			user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow;

			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				return Result<bool>.Fail($"Could not logout user: {errors}");
			}

			return Result<bool>.Ok(true);
		}
		catch (Exception ex)
		{
			return Result<bool>.Fail($"An error occurred during logout: {ex.Message}");
		}
	}

	private async Task<Result<AuthResponseDto>> GenerateUserTokenAsync(AppUser user)
	{
		var jwtToken = GenerateJwtToken(user);
		var refreshToken = GenerateRefreshToken();

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenValidityInDays);

		var updateResult = await _userManager.UpdateAsync(user);
		if (!updateResult.Succeeded)
		{
			var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
			return Result<AuthResponseDto>.Fail($"Failed to save session: {errors}");
		}

		return Result<AuthResponseDto>.Ok(new AuthResponseDto
		{
			Token = jwtToken,
			RefreshToken = refreshToken,
			UserId = user.Id,
			Email = user.Email!,
			FirstName = user.FirstName,
			LastName = user.LastName,
			ProfilePictureUrl = user.ProfilePictureUrl,
			DateOfBirth = user.DateOfBirth
		});
	}

	private string GenerateJwtToken(AppUser user)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id),
			new Claim(JwtRegisteredClaimNames.Email, user.Email!),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Email, user.Email!),
		};

		var token = new JwtSecurityToken(
			issuer: _jwt.Issuer,
			audience: _jwt.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_jwt.TokenValidityInMinutes),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	private static string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
	{
		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateAudience = false,
			ValidateIssuer = false,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret)),
			ValidateLifetime = false
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

		if (securityToken is not JwtSecurityToken jwtSecurityToken ||
		    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
		{
			throw new SecurityTokenException("Invalid token");
		}

		return principal;
	}
}
