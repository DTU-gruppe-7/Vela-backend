using Vela.Domain.Enums;

namespace Vela.Domain.Entities.Notification;

public class Notification
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public string? Message { get; set; }
    public NotificationType NotificationType { get; set; }
    public Guid RelatedEntityId  { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}