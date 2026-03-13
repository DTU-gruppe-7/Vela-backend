using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vela.Application.Common;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;
using Vela.Infrastructure.Identity;

namespace Vela.Infrastructure.Identity;

public class AuthService(UserManager<AppUser> userManager, IConfiguration configuration) : IAuthService
{
    private readonly UserManager<AppUser> _userManager =  userManager;
    private readonly IConfiguration _configuration =  configuration;


    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto requestDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(requestDto.Email);
        if (existingUser != null)
            return Result<AuthResponseDto>.Fail("A user with this email does already exists");

        var newUser = new AppUser
        {
            UserName = requestDto.Email,
            Email = requestDto.Email,
            FirstName = requestDto.FirstName,
            LastName = requestDto.LastName,
            DateOfBirth = requestDto.DateOfBirth,
        };
        
        var result  = await _userManager.CreateAsync(newUser, requestDto.Password);

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
        {
            return Result<AuthResponseDto>.Fail("Wrong username or password.");
        }

        return await GenerateUserTokenAsync(user);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto)
    {
        var principal = GetPrincipalFromExpiredToken(requestDto.Token);
        if (principal == null)
            return Result<AuthResponseDto>.Fail("Invalid token");

        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Result<AuthResponseDto>.Fail("Invalid token.");

        var user = await _userManager.FindByEmailAsync(email);

        // Tjek at refresh token matcher og ikke er udløbet
        if (user == null || user.RefreshToken != requestDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Result<AuthResponseDto>.Fail("Invalid or  expired token. Please login again.");
        }

        // Generer nye tokens
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
        } catch (Exception ex)
        {
            return Result.Fail($"An error occured while deleting user: {ex.Message}");
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
            user.RefreshTokenExpiryTime = DateTime.UtcNow;
            
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
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["JwtSettings:RefreshTokenValidityInDays"] ?? "30"));
        
        await _userManager.UpdateAsync(user);
        
        return Result<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Token = jwtToken,
            RefreshToken = refreshToken,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            DateOfBirth = user.DateOfBirth
        });
    }

    private string GenerateJwtToken(AppUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id), // Vigtig for ASP.NET's indbyggede User.Identity.Name
            new Claim(ClaimTypes.Email, user.Email!),
            // Tilføj flere claims her (f.eks. GroupId, Roles) når du har brug for det
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:TokenValidityInMinutes"] ?? "60")),
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!)),
            ValidateLifetime = false // Vi vil netop gerne læse den, selvom den er udløbet
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