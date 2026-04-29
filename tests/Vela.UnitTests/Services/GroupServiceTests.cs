using FluentAssertions;
using Moq;
using Vela.Application.Common;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Application.Services;
using Vela.Domain.Entities.Group;
using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Entities.ShoppingList;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class GroupServiceTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<IMealPlanRepository> _mealPlanRepo = new();
    private readonly Mock<IShoppingListRepository> _shoppingListRepo = new();
    private readonly Mock<ILikeRepository> _likeRepo = new();
    private readonly Mock<ILikeService> _likeService = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IGroupAuthorizationService> _groupAuth = new();
    private readonly GroupService _sut;

    public GroupServiceTests()
    {
        _sut = new GroupService(
            _groupRepo.Object,
            _mealPlanRepo.Object,
            _shoppingListRepo.Object,
            _likeRepo.Object,
            _likeService.Object,
            _userRepo.Object,
            _groupAuth.Object);
    }

    [Fact]
    public async Task CreateGroupWithResourcesAsync_CreatesGroupMealPlanShoppingListAndRecalculates()
    {
        var request = new CreateGroupRequest { Name = "Test Group" };
        var profiles = new Dictionary<string, (string FirstName, string LastName, string Email)>
        {
            { "user-1", ("John", "Doe", "john@example.com") }
        };
        _userRepo.Setup(x => x.GetUserProfilesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(profiles);

        var result = await _sut.CreateGroupWithResourcesAsync("user-1", request);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Test Group");
        result.Data.Members.Should().HaveCount(1);
        result.Data.Members.First().Role.Should().Be(GroupRole.Owner);

        _groupRepo.Verify(x => x.AddAsync(It.Is<Group>(g => g.Name == "Test Group")), Times.Once);
        _mealPlanRepo.Verify(x => x.AddAsync(It.Is<MealPlan>(m => m.Name == "Test Group Meal Plan")), Times.Once);
        _shoppingListRepo.Verify(x => x.AddAsync(It.Is<ShoppingList>(s => s.Name == "Test Group Shopping List")), Times.Once);
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        _likeService.Verify(x => x.RecalculateGroupMatchesAsync(It.IsAny<Group>()), Times.Once);
    }

    [Fact]
    public async Task GetGroupAsync_WhenGroupNotFound_ReturnsFail()
    {
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);

        var result = await _sut.GetGroupAsync(Guid.NewGuid(), "user-1");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task GetGroupAsync_WhenUnauthorized_ReturnsForbidden()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeMembership(group, "stranger")).Returns(Result.Fail("Nope", ResultErrorType.Forbidden));

        var result = await _sut.GetGroupAsync(group.Id, "stranger");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Forbidden);
    }

    [Fact]
    public async Task DeleteGroupAsync_WhenAuthorized_DeletesGroup()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeDeleteGroup(group, "owner-1")).Returns(Result.Ok());

        var result = await _sut.DeleteGroupAsync(group.Id, "owner-1");

        result.Success.Should().BeTrue();
        _groupRepo.Verify(x => x.DeleteAsync(group), Times.Once);
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupAsync_WhenUnauthorized_ReturnsFail()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeDeleteGroup(group, "user-1")).Returns(Result.Fail("Not owner"));

        var result = await _sut.DeleteGroupAsync(group.Id, "user-1");

        result.Success.Should().BeFalse();
        _groupRepo.Verify(x => x.DeleteAsync(It.IsAny<Group>()), Times.Never);
    }

    [Fact]
    public async Task AddMemberAsync_WhenGroupExists_AddsMemberAndRecalculates()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Test", Members = new List<GroupMember>() };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);

        var result = await _sut.AddMemberAsync(group.Id, "user-2");

        result.Success.Should().BeTrue();
        group.Members.Should().ContainSingle(m => m.UserId == "user-2" && m.Role == GroupRole.Member);
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        _likeService.Verify(x => x.RecalculateGroupMatchesAsync(group), Times.Once);
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenAuthorized_RemovesAndRecalculates()
    {
        var targetMember = new GroupMember { UserId = "user-2", GroupId = Guid.NewGuid(), Role = GroupRole.Member };
        var group = new Group { Id = targetMember.GroupId, Name = "Test", Members = new List<GroupMember> { targetMember } };
        
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeRemoveMember(group, "owner-1", "user-2")).Returns(Result.Ok());

        var result = await _sut.RemoveMemberAsync(group.Id, "user-2", "owner-1");

        result.Success.Should().BeTrue();
        group.Members.Should().BeEmpty();
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        _likeService.Verify(x => x.RecalculateGroupMatchesAsync(group), Times.Once);
    }

    [Fact]
    public async Task ChangeMemberRoleAsync_WhenAuthorized_UpdatesRole()
    {
        var targetMember = new GroupMember { UserId = "user-2", GroupId = Guid.NewGuid(), Role = GroupRole.Member };
        var group = new Group { Id = targetMember.GroupId, Name = "Test", Members = new List<GroupMember> { targetMember } };

        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeChangeMemberRole(group, "owner-1", "user-2", GroupRole.Administrator)).Returns(Result.Ok());

        var result = await _sut.ChangeMemberRoleAsync(group.Id, "user-2", GroupRole.Administrator, "owner-1");

        result.Success.Should().BeTrue();
        targetMember.Role.Should().Be(GroupRole.Administrator);
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LeaveGroupAsync_WhenAuthorized_RemovesCaller()
    {
        var caller = new GroupMember { UserId = "user-1", GroupId = Guid.NewGuid(), Role = GroupRole.Member };
        var group = new Group { Id = caller.GroupId, Name = "Test", Members = new List<GroupMember> { caller } };

        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeLeaveGroup(group, "user-1")).Returns(Result.Ok());

        var result = await _sut.LeaveGroupAsync(group.Id, "user-1");

        result.Success.Should().BeTrue();
        group.Members.Should().BeEmpty();
        _likeService.Verify(x => x.RecalculateGroupMatchesAsync(group), Times.Once);
    }

    [Fact]
    public async Task LeaveGroupAsync_WhenOwnerTriesToLeave_ReturnsFail()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Test" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeLeaveGroup(group, "owner-1")).Returns(Result.Fail("Owner must transfer"));

        var result = await _sut.LeaveGroupAsync(group.Id, "owner-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("transfer");
    }

    [Fact]
    public async Task TransferOwnershipAsync_WhenAuthorized_SwapsRoles()
    {
        var caller = new GroupMember { UserId = "owner-1", GroupId = Guid.NewGuid(), Role = GroupRole.Owner };
        var target = new GroupMember { UserId = "user-2", GroupId = caller.GroupId, Role = GroupRole.Member };
        var group = new Group { Id = caller.GroupId, Name = "Test", Members = new List<GroupMember> { caller, target } };

        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeTransferOwnership(group, "owner-1", "user-2")).Returns(Result.Ok());

        var result = await _sut.TransferOwnershipAsync(group.Id, "user-2", "owner-1");

        result.Success.Should().BeTrue();
        caller.Role.Should().Be(GroupRole.Administrator);
        target.Role.Should().Be(GroupRole.Owner);
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupNameAsync_WhenEmptyName_ReturnsFail()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Old Name" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeMembership(group, "user-1")).Returns(Result.Ok());

        var result = await _sut.UpdateGroupNameAsync(group.Id, "", "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("tomt");
    }

    [Fact]
    public async Task UpdateGroupNameAsync_WhenValid_UpdatesName()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Old Name" };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(group.Id)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeMembership(group, "user-1")).Returns(Result.Ok());

        var result = await _sut.UpdateGroupNameAsync(group.Id, " New Name ", "user-1");

        result.Success.Should().BeTrue();
        group.Name.Should().Be("New Name");
        _groupRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
