using Microsoft.AspNetCore.Identity;

namespace Vela.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    
    // App-specifikke indstillinger
    public int DefaultPortions { get; set; } = 2; // Standard antal personer de handler ind til
    public string? PushNotificationToken { get; set; } // Til fremtidige Tinder-matches
    
    // Audit og administration
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
}