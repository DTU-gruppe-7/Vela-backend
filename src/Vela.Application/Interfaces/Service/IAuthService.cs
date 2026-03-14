using Vela.Application.Common;
using Vela.Application.DTOs.Auth;

namespace Vela.Application.Interfaces.Service;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto requestDto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto requestDto);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto);
    Task<Result> DeleteUserAsync(string userId);
    Task<Result<bool>> LogoutAsync(string userId);
}