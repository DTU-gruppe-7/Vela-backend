using Vela.Domain.Enums;

namespace Vela.Application.Interfaces.Service.Notification;

public interface INotificationDispatcher
{
    Task DispatchAsync(string userId, string title, string message, NotificationType type, Guid relatedEntityId);
}