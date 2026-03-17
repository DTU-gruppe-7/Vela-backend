using Vela.Application.Common;
using Vela.Application.DTOs;
using Vela.Application.DTOs.Notification;

namespace Vela.Application.Interfaces.Service.Notification;

public interface INotificationService
{
    Task<Result<IEnumerable<NotificationDto>>> GetNotificationsAsync(string userId);
    Task<Result> MarkAsReadAsync(Guid notificationId, string userId);
}