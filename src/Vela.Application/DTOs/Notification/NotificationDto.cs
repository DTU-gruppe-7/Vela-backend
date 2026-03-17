using Vela.Domain.Enums;

namespace Vela.Application.DTOs.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}