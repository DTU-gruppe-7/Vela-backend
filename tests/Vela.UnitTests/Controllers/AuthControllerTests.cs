using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Vela.API.Controllers;
using Vela.Application.Common;
using Vela.Application.Configuration;
using Vela.Application.DTOs.Auth;
using Vela.Application.Interfaces.Service;

namespace Vela.UnitTests.Controllers;

public class AuthControllerTests
{
	private const string RefreshTokenCookieName = "refreshToken";
	private const string RefreshTokenCookiePath = "/api/v1/Auth";

	private readonly Mock<IAuthService> _authServiceMock = new();
	private readonly Mock<IUserOnboardingService> _userOnboardingServiceMock = new();
	private readonly Mock<IUserProfileService> _userProfileServiceMock = new();
	private readonly Mock<IWebHostEnvironment> _environmentMock = new();
	private readonly IOptions<JwtSettings> _jwtSettings;

	public AuthControllerTests()
	{
		_environmentMock.SetupGet(x => x.EnvironmentName).Returns(Environments.Production);
		_jwtSettings = Options.Create(new JwtSettings { RefreshTokenValidityInDays = 30 });
	}

	[Fact]
	public async Task Register_WhenSuccessful_ReturnsAccessTokenAndUserAndSetsRefreshCookie()
	{
		// Arrange
		var controller = CreateController();
		var registerRequest = new RegisterRequestDto
		{
			Email = "newuser@example.com",
			Password = "Password123!",
			FirstName = "New",
			LastName = "User",
			DateOfBirth = new DateOnly(1995, 5, 15)
		};

		_userOnboardingServiceMock
			.Setup(x => x.RegisterAsync(registerRequest))
			.ReturnsAsync(Result<AuthResponseDto>.Ok(new AuthResponseDto
			{
				Token = "access-token",
				RefreshToken = "refresh-token",
				UserId = "new-user-id",
				Email = "newuser@example.com",
				FirstName = "New",
				LastName = "User",
				ProfilePictureUrl = null,
				DateOfBirth = new DateOnly(1995, 5, 15)
			}));

		// Act
		var result = await controller.Register(registerRequest);

		// Assert
		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var response = okResult.Value.Should().BeOfType<AuthLoginResponseDto>().Subject;
		response.AccessToken.Should().Be("access-token");
		response.User.UserId.Should().Be("new-user-id");
		response.User.Email.Should().Be("newuser@example.com");
		response.User.FirstName.Should().Be("New");
		response.User.LastName.Should().Be("User");
		response.User.DateOfBirth.Should().Be(new DateOnly(1995, 5, 15));

		var setCookieHeader = controller.Response.Headers["Set-Cookie"].ToString();
		setCookieHeader.Should().Contain($"{RefreshTokenCookieName}=refresh-token");
		setCookieHeader.ToLowerInvariant().Should().Contain("httponly");
		setCookieHeader.ToLowerInvariant().Should().Contain("secure");
		setCookieHeader.ToLowerInvariant().Should().Contain("samesite=none");
		setCookieHeader.ToLowerInvariant().Should().Contain($"path={RefreshTokenCookiePath.ToLowerInvariant()}");
		setCookieHeader.ToLowerInvariant().Should().Contain("expires=");
	}

	[Fact]
	public async Task Register_WhenServiceFails_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		var registerRequest = new RegisterRequestDto
		{
			Email = "existing@example.com",
			Password = "Password123!",
			FirstName = "Test",
			LastName = "User"
		};

		_userOnboardingServiceMock
			.Setup(x => x.RegisterAsync(registerRequest))
			.ReturnsAsync(Result<AuthResponseDto>.Fail("Registration failed. Please check your details."));

		// Act
		var result = await controller.Register(registerRequest);

		// Assert
		result.Result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task Login_WhenSuccessful_ReturnsAccessTokenAndUserAndSetsRefreshCookie()
	{
		// Arrange
		var controller = CreateController();
		var loginRequest = new LoginRequestDto
		{
			Email = "test@example.com",
			Password = "Password123!"
		};

		_authServiceMock
			.Setup(x => x.LoginAsync(loginRequest))
			.ReturnsAsync(Result<AuthResponseDto>.Ok(new AuthResponseDto
			{
				Token = "access-token",
				RefreshToken = "refresh-token",
				UserId = "user-id",
				Email = "test@example.com",
				FirstName = "Test",
				LastName = "User",
				ProfilePictureUrl = "https://example.com/profile.jpg",
				DateOfBirth = new DateOnly(1990, 1, 2)
			}));

		// Act
		var result = await controller.Login(loginRequest);

		// Assert
		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var response = okResult.Value.Should().BeOfType<AuthLoginResponseDto>().Subject;
		response.AccessToken.Should().Be("access-token");
		response.User.UserId.Should().Be("user-id");
		response.User.Email.Should().Be("test@example.com");
		response.User.FirstName.Should().Be("Test");
		response.User.LastName.Should().Be("User");
		response.User.ProfilePictureUrl.Should().Be("https://example.com/profile.jpg");
		response.User.DateOfBirth.Should().Be(new DateOnly(1990, 1, 2));

		var setCookieHeader = controller.Response.Headers["Set-Cookie"].ToString();
		setCookieHeader.Should().Contain($"{RefreshTokenCookieName}=refresh-token");
		setCookieHeader.ToLowerInvariant().Should().Contain("httponly");
		setCookieHeader.ToLowerInvariant().Should().Contain("secure");
		setCookieHeader.ToLowerInvariant().Should().Contain("samesite=none");
		setCookieHeader.ToLowerInvariant().Should().Contain($"path={RefreshTokenCookiePath.ToLowerInvariant()}");
		setCookieHeader.ToLowerInvariant().Should().Contain("expires=");
	}

	[Fact]
	public async Task RefreshToken_WhenRequestBodyIsMissingRefreshToken_UsesRefreshTokenCookie()
	{
		// Arrange
		var controller = CreateController();
		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};
		controller.Request.Headers.Cookie = $"{RefreshTokenCookieName}=cookie-refresh-token";

		var refreshRequest = new RefreshTokenRequestDto
		{
			AccessToken = "expired-access-token"
		};

		_authServiceMock
			.Setup(x => x.RefreshTokenAsync("expired-access-token", "cookie-refresh-token"))
			.ReturnsAsync(Result<AuthResponseDto>.Ok(new AuthResponseDto
			{
				Token = "new-access-token",
				RefreshToken = "new-refresh-token",
				UserId = "user-id",
				Email = "test@example.com",
				FirstName = "Test",
				LastName = "User"
			}));

		// Act
		var result = await controller.RefreshToken(refreshRequest);

		// Assert
		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var response = okResult.Value.Should().BeOfType<AuthLoginResponseDto>().Subject;
		response.AccessToken.Should().Be("new-access-token");
		response.User.UserId.Should().Be("user-id");

		var setCookieHeader = controller.Response.Headers["Set-Cookie"].ToString();
		setCookieHeader.Should().Contain($"{RefreshTokenCookieName}=new-refresh-token");
		setCookieHeader.ToLowerInvariant().Should().Contain("httponly");
		setCookieHeader.ToLowerInvariant().Should().Contain($"path={RefreshTokenCookiePath.ToLowerInvariant()}");
	}

	[Fact]
	public async Task RefreshToken_WhenCookieIsMissing_ReturnsUnauthorized()
	{
		// Arrange
		var controller = CreateController();
		var refreshRequest = new RefreshTokenRequestDto
		{
			AccessToken = "expired-access-token"
		};

		// Act
		var result = await controller.RefreshToken(refreshRequest);

		// Assert
		var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
		unauthorizedResult.Value.Should().Be("Missing refresh token cookie.");
		_authServiceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task RefreshToken_WhenServiceFails_ReturnsUnauthorized()
	{
		// Arrange
		var controller = CreateController();
		controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
		controller.Request.Headers.Cookie = $"{RefreshTokenCookieName}=some-token";

		var refreshRequest = new RefreshTokenRequestDto { AccessToken = "tampered-or-expired-token" };

		_authServiceMock
			.Setup(x => x.RefreshTokenAsync("tampered-or-expired-token", "some-token"))
			.ReturnsAsync(Result<AuthResponseDto>.Fail("Invalid or expired token. Please login again."));

		// Act
		var result = await controller.RefreshToken(refreshRequest);

		// Assert
		result.Result.Should().BeOfType<UnauthorizedObjectResult>();
	}

	[Fact]
	public async Task Logout_WhenSuccessful_ClearsRefreshCookie()
	{
		// Arrange
		var controller = CreateController();
		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				User = new ClaimsPrincipal(new ClaimsIdentity(
					[new Claim(ClaimTypes.NameIdentifier, "user-id")],
					"Test"))
			}
		};

		_authServiceMock
			.Setup(x => x.LogoutAsync("user-id"))
			.ReturnsAsync(Result<bool>.Ok(true));

		// Act
		var result = await controller.Logout();

		// Assert
		result.Should().BeOfType<NoContentResult>();

		var setCookieHeader = controller.Response.Headers["Set-Cookie"].ToString();
		setCookieHeader.ToLowerInvariant().Should().Contain("refreshtoken=");
		setCookieHeader.ToLowerInvariant().Should().Contain("expires=");
		setCookieHeader.ToLowerInvariant().Should().Contain($"path={RefreshTokenCookiePath.ToLowerInvariant()}");
	}

	[Fact]
	public async Task Logout_WhenServiceFails_ReturnsBadRequest()
	{
		// Arrange
		var controller = CreateController();
		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				User = new ClaimsPrincipal(new ClaimsIdentity(
					[new Claim(ClaimTypes.NameIdentifier, "user-id")],
					"Test"))
			}
		};

		_authServiceMock
			.Setup(x => x.LogoutAsync("user-id"))
			.ReturnsAsync(Result<bool>.Fail("Could not logout user."));

		// Act
		var result = await controller.Logout();

		// Assert
		result.Should().BeOfType<BadRequestObjectResult>();
	}

	private AuthController CreateController()
	{
		var controller = new AuthController(
			_authServiceMock.Object,
			_userOnboardingServiceMock.Object,
			_userProfileServiceMock.Object,
			_environmentMock.Object,
			_jwtSettings);

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};

		return controller;
	}
}
