using Vela.Domain.Enums;

namespace Vela.Application.Interfaces.Service.Notification;

public interface IRealtimeNotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, NotificationType type, object? payload = null);
}