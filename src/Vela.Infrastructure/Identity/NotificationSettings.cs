namespace Vela.Infrastructure.Identity;

public class NotificationSettings
{
    public required string UserId { get; set; }
    public bool ReceivePushNotifications { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; } = false;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}