using Vela.Application.Common;
using Vela.Application.DTOs.Notification;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Domain.Entities.Notification;

namespace Vela.Application.Services.Notification;

public class NotificationService(INotificationRepository notificationRepository) : INotificationService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    
    public async Task<Result<IEnumerable<NotificationDto>>> GetNotificationsAsync(string userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message ?? string.Empty,
            Type = n.NotificationType.ToString(),
            RelatedEntityId = n.RelatedEntityId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        return Result<IEnumerable<NotificationDto>>.Ok(dtos);
    }

    public async Task<Result> MarkAsReadAsync(Guid notificationId, string userId)
    {
        var notification = await _notificationRepository.GetByUuidAsync(notificationId);
        if (notification == null)
            return Result.Fail("Notification not found");

        // Sikkerhedstjek: Tjek at brugeren rent faktisk ejer denne notifikation
        if (notification.UserId != userId)
            return Result.Fail("Unauthorized to modify this notification");

        notification.IsRead = true;
        
        await _notificationRepository.SaveChangesAsync();

        return Result.Ok();
    }
}