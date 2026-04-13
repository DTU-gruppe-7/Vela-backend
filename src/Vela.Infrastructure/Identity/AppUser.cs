﻿using Microsoft.AspNetCore.Identity;
using Vela.Domain.Entities;

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

    // Kostpræferencer
    public bool AvoidGluten { get; set; }
    public bool AvoidLactose { get; set; }
    public bool AvoidNuts { get; set; }
    public bool IsVegan { get; set; }
    
    // Audit og administration
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
    
}