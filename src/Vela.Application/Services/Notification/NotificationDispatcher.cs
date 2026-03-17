using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Domain.Enums;

namespace Vela.Application.Services.Notification;

public class NotificationDispatcher(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService) : INotificationDispatcher
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;
    
    public async Task DispatchAsync(string userId, string title, string message, NotificationType type, Guid relatedEntityId)
    {
        var notification = new Domain.Entities.Notification.Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();
        
        //Her kan der sættes flere services ind (e-mail, sms, push
        
        await _realtimeNotificationService.SendNotificationAsync(userId, title, message, type,
            new { NotificationId = notification.Id, RelatedEntityId = relatedEntityId});
    }
    
    
}