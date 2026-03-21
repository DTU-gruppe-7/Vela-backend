using Microsoft.AspNetCore.SignalR;
using Vela.API.Hubs;
using Vela.Application.DTOs.Notification;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Domain.Enums;

namespace Vela.API.Notification;

public class SignalRNotificationService(IHubContext<NotificationHub> hubContext) : IRealtimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    public async Task SendNotificationAsync(string userId, string title, string message, NotificationType type, object? payload = null)
    {
        await _hubContext.Clients.Group(userId)
            .SendAsync("ReceiveNotification", new SignalRNotificationDto
            {
                Title = title,
                Message = message,
                Type = type,
                Payload = payload,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}