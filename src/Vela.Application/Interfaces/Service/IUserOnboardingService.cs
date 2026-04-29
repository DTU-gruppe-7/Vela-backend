using Vela.Application.Common;
using Vela.Application.DTOs.Auth;

namespace Vela.Application.Interfaces.Service;

public interface IUserOnboardingService
{
	Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto requestDto);
}
