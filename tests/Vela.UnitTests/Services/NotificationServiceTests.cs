using FluentAssertions;
using Moq;
using Vela.Application.Common;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Services.Notification;
using Vela.Domain.Entities.Notification;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepo = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_notificationRepo.Object);
    }

    [Fact]
    public async Task GetNotificationsAsync_ReturnsMappedDtos()
    {
        var notifications = new List<Notification>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "user-1",
                Title = "Nyt Match!",
                Message = "Du har et match",
                NotificationType = NotificationType.NewMatch,
                RelatedEntityId = Guid.NewGuid(),
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _notificationRepo.Setup(x => x.GetByUserIdAsync("user-1")).ReturnsAsync(notifications);

        var result = await _sut.GetNotificationsAsync("user-1");

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data!.First().Title.Should().Be("Nyt Match!");
        result.Data!.First().Type.Should().Be("NewMatch");
        result.Data!.First().IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationNotFound_ReturnsFail()
    {
        _notificationRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid>())).ReturnsAsync((Notification?)null);

        var result = await _sut.MarkAsReadAsync(Guid.NewGuid(), "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenUserIsNotOwner_ReturnsFail()
    {
        var notification = new Notification { Id = Guid.NewGuid(), UserId = "owner-1", Title = "Title" };
        _notificationRepo.Setup(x => x.GetByUuidAsync(notification.Id)).ReturnsAsync(notification);

        var result = await _sut.MarkAsReadAsync(notification.Id, "intruder");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenAuthorized_MarksAsReadAndSaves()
    {
        var notification = new Notification { Id = Guid.NewGuid(), UserId = "owner-1", Title = "Title", IsRead = false };
        _notificationRepo.Setup(x => x.GetByUuidAsync(notification.Id)).ReturnsAsync(notification);

        var result = await _sut.MarkAsReadAsync(notification.Id, "owner-1");

        result.Success.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _notificationRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
