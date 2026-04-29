using FluentAssertions;
using Moq;
using Vela.Application.Common;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Application.Services;
using Vela.Domain.Entities.Group;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class GroupInviteServiceTests
{
    private readonly Mock<IGroupInviteRepository> _inviteRepo = new();
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<IGroupService> _groupService = new();
    private readonly Mock<IGroupAuthorizationService> _groupAuth = new();
    private readonly Mock<INotificationDispatcher> _notificationDispatcher = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly GroupInviteService _sut;

    public GroupInviteServiceTests()
    {
        _sut = new GroupInviteService(
            _inviteRepo.Object,
            _groupRepo.Object,
            _groupService.Object,
            _groupAuth.Object,
            _notificationDispatcher.Object,
            _userRepo.Object);
    }

    [Fact]
    public async Task SendInviteAsync_WhenGroupNotFound_ReturnsFail()
    {
        _userRepo.Setup(x => x.FindUserIdByEmailAsync("test@test.com")).ReturnsAsync("user-2");
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);

        var result = await _sut.SendInviteAsync("test@test.com", Guid.NewGuid(), "caller");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task SendInviteAsync_WhenUnauthorized_ReturnsFail()
    {
        _userRepo.Setup(x => x.FindUserIdByEmailAsync("test@test.com")).ReturnsAsync("user-2");
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeSendInvite(group, "caller")).Returns(Result.Fail("Unauthorized", ResultErrorType.Forbidden));

        var result = await _sut.SendInviteAsync("test@test.com", group.Id, "caller");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Forbidden);
    }

    [Fact]
    public async Task SendInviteAsync_WhenInviteExists_ReturnsFail()
    {
        _userRepo.Setup(x => x.FindUserIdByEmailAsync("test@test.com")).ReturnsAsync("user-2");
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeSendInvite(group, "caller")).Returns(Result.Ok());
        _inviteRepo.Setup(x => x.GetGroupInviteAsync("user-2", group.Id)).ReturnsAsync(new GroupInvite { GroupId = group.Id, UserId = "user-2" });

        var result = await _sut.SendInviteAsync("test@test.com", group.Id, "caller");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("allerede sendt en invitation");
    }

    [Fact]
    public async Task SendInviteAsync_WhenAuthorizedAndNew_SendsInviteAndNotification()
    {
        _userRepo.Setup(x => x.FindUserIdByEmailAsync("test@test.com")).ReturnsAsync("user-2");
        var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeSendInvite(group, "caller")).Returns(Result.Ok());
        _inviteRepo.Setup(x => x.GetGroupInviteAsync("user-2", group.Id)).ReturnsAsync((GroupInvite?)null);

        var result = await _sut.SendInviteAsync("test@test.com", group.Id, "caller");

        result.Success.Should().BeTrue();
        _inviteRepo.Verify(x => x.AddAsync(It.Is<GroupInvite>(i => i.UserId == "user-2" && i.GroupId == group.Id)), Times.Once);
        _notificationDispatcher.Verify(x => x.DispatchAsync("user-2", It.IsAny<string>(), It.IsAny<string>(), NotificationType.GroupInvite, group.Id), Times.Once);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenInviteExists_AddsMemberAndDeletesInvite()
    {
        var groupId = Guid.NewGuid();
        var invite = new GroupInvite { GroupId = groupId, UserId = "user-1" };
        var group = new Group { Id = groupId, Name = "Test" };
        
        _groupRepo.Setup(x => x.GetByUuidAsync(groupId)).ReturnsAsync(group);
        _inviteRepo.Setup(x => x.GetGroupInviteAsync("user-1", groupId)).ReturnsAsync(invite);
        _groupService.Setup(x => x.AddMemberAsync(groupId, "user-1")).ReturnsAsync(Result.Ok());

        var result = await _sut.AcceptInviteAsync("user-1", groupId);

        result.Success.Should().BeTrue();
        _groupService.Verify(x => x.AddMemberAsync(groupId, "user-1"), Times.Once);
        _inviteRepo.Verify(x => x.DeleteInviteAsync(invite), Times.Once);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenGroupNotFound_ReturnsFail()
    {
        _groupRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);

        var result = await _sut.AcceptInviteAsync("user-1", Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenInviteNotFound_ReturnsFail()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetByUuidAsync(group.Id)).ReturnsAsync(group);
        _inviteRepo.Setup(x => x.GetGroupInviteAsync("user-1", group.Id)).ReturnsAsync((GroupInvite?)null);

        var result = await _sut.AcceptInviteAsync("user-1", group.Id);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task DeclineInviteAsync_WhenInviteExists_DeletesInvite()
    {
        var groupId = Guid.NewGuid();
        var invite = new GroupInvite { GroupId = groupId, UserId = "user-1" };
        _inviteRepo.Setup(x => x.GetGroupInviteAsync("user-1", groupId)).ReturnsAsync(invite);

        var result = await _sut.DeclineInviteAsync("user-1", groupId);

        result.Success.Should().BeTrue();
        _inviteRepo.Verify(x => x.DeleteInviteAsync(invite), Times.Once);
    }

    [Fact]
    public async Task GetInvitesByUserIdAsync_ReturnsMappedDtos()
    {
        var invites = new List<GroupInvite> { new() { GroupId = Guid.NewGuid(), UserId = "user-1" } };
        _inviteRepo.Setup(x => x.GetInvitesByUserIdAsync("user-1")).ReturnsAsync(invites);

        var result = await _sut.GetInvitesByUserIdAsync("user-1");

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetInvitesByGroupIdAsync_WhenAuthorized_ReturnsMappedDtos()
    {
        var groupId = Guid.NewGuid();
        var group = new Group { Id = groupId, Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(groupId)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeViewInvites(group, "caller")).Returns(Result.Ok());
        
        var invites = new List<GroupInvite> { new() { GroupId = groupId, UserId = "user-1" } };
        _inviteRepo.Setup(x => x.GetInvitesByGroupIdAsync(groupId)).ReturnsAsync(invites);

        var result = await _sut.GetInvitesByGroupIdAsync(groupId, "caller");

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
}
