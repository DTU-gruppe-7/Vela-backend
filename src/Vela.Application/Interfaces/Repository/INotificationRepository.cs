using Vela.Domain.Entities.Notification;

namespace Vela.Application.Interfaces.Repository;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
}