using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities.Notification;
using Vela.Infrastructure.Data;

namespace Vela.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext context) : Repository<Notification>(context), INotificationRepository
{
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
    {
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}