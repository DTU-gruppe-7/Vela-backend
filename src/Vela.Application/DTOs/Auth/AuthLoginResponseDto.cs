namespace Vela.Application.DTOs.Auth;

public sealed record AuthLoginResponseDto(string AccessToken, AuthUserDto User);

public sealed record AuthUserDto(
	string UserId,
	string Email,
	string FirstName,
	string LastName,
	string? ProfilePictureUrl,
	DateOnly? DateOfBirth);


