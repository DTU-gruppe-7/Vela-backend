namespace Vela.Application.Configuration;

public class JwtSettings
{
	public string Secret { get; set; } = string.Empty;
	public string Issuer { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
	public double TokenValidityInMinutes { get; set; } = 60;
	public double RefreshTokenValidityInDays { get; set; } = 30;
}
