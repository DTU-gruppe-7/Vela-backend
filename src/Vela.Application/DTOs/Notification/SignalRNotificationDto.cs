using System.Text.Json.Serialization;
using Vela.Domain.Enums;

namespace Vela.Application.DTOs.Notification;

public class SignalRNotificationDto
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; set; }
    public object? Payload { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}