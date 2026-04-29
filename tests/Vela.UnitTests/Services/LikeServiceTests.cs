using FluentAssertions;
using Moq;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Application.Services;
using Vela.Domain.Entities;
using Vela.Domain.Entities.Group;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;
using MatchEntity = Vela.Domain.Entities.Match;

namespace Vela.UnitTests.Services;

public class LikeServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepo = new();
    private readonly Mock<ILikeRepository> _likeRepo = new();
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<INotificationDispatcher> _notificationDispatcher = new();
    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _sut = new LikeService(
            _recipeRepo.Object,
            _likeRepo.Object,
            _groupRepo.Object,
            _notificationDispatcher.Object);
    }

    [Fact]
    public async Task RecordSwipeAsync_WhenRecipeNotFound_ReturnsFail()
    {
        _recipeRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid?>())).ReturnsAsync((Recipe?)null);

        var result = await _sut.RecordSwipeAsync("user-1", new SwipeDto { RecipeId = Guid.NewGuid(), Direction = SwipeDirection.Like });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Recipe not found");
    }

    [Fact]
    public async Task RecordSwipeAsync_WhenAlreadySwiped_ReturnsOkWithoutAddingNewSwipe()
    {
        var recipeId = Guid.NewGuid();
        _recipeRepo.Setup(x => x.GetByUuidAsync(recipeId)).ReturnsAsync(new Recipe { Id = recipeId, Name = "Test" });
        _likeRepo.Setup(x => x.HasUserSwipedOnRecipeAsync("user-1", recipeId)).ReturnsAsync(true);

        var result = await _sut.RecordSwipeAsync("user-1", new SwipeDto { RecipeId = recipeId, Direction = SwipeDirection.Like });

        result.Success.Should().BeTrue();
        _likeRepo.Verify(x => x.AddAsync(It.IsAny<Like>()), Times.Never);
    }

    [Fact]
    public async Task RecordSwipeAsync_WhenValidSwipe_AddsSwipeAndChecksMatches()
    {
        var recipeId = Guid.NewGuid();
        _recipeRepo.Setup(x => x.GetByUuidAsync(recipeId)).ReturnsAsync(new Recipe { Id = recipeId, Name = "Test" });
        _likeRepo.Setup(x => x.HasUserSwipedOnRecipeAsync("user-1", recipeId)).ReturnsAsync(false);
        _groupRepo.Setup(x => x.GetGroupsByUserIdAsync("user-1")).ReturnsAsync(new List<Group>());

        var result = await _sut.RecordSwipeAsync("user-1", new SwipeDto { RecipeId = recipeId, Direction = SwipeDirection.Like });

        result.Success.Should().BeTrue();
        _likeRepo.Verify(x => x.AddAsync(It.Is<Like>(l => l.RecipeId == recipeId && l.UserId == "user-1")), Times.Once);
        _likeRepo.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RecordSwipeAsync_WhenCreatesNewGroupMatch_RecordsMatchAndDispatchesNotifications()
    {
        var recipeId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var group = new Group
        {
            Id = groupId, Name = "Familie",
            Members = new List<GroupMember>
            {
                new() { GroupId = groupId, UserId = "user-1", Role = GroupRole.Member },
                new() { GroupId = groupId, UserId = "user-2", Role = GroupRole.Member }
            }
        };

        _recipeRepo.Setup(x => x.GetByUuidAsync(recipeId)).ReturnsAsync(new Recipe { Id = recipeId, Name = "Test" });
        _likeRepo.Setup(x => x.HasUserSwipedOnRecipeAsync("user-1", recipeId)).ReturnsAsync(false);
        _groupRepo.Setup(x => x.GetGroupsByUserIdAsync("user-1")).ReturnsAsync(new List<Group> { group });
        _likeRepo.Setup(x => x.CheckForNewMatch(It.IsAny<IEnumerable<string>>(), recipeId)).ReturnsAsync(true);
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(groupId)).ReturnsAsync(group);

        var result = await _sut.RecordSwipeAsync("user-1", new SwipeDto { RecipeId = recipeId, Direction = SwipeDirection.Like });

        result.Success.Should().BeTrue();
        _likeRepo.Verify(x => x.RecordMatchAsync(It.Is<MatchEntity>(m => m.GroupId == groupId && m.RecipeId == recipeId)), Times.Once);
        _notificationDispatcher.Verify(x => x.DispatchAsync("user-1", It.IsAny<string>(), It.IsAny<string>(), NotificationType.NewMatch, recipeId), Times.Once);
        _notificationDispatcher.Verify(x => x.DispatchAsync("user-2", It.IsAny<string>(), It.IsAny<string>(), NotificationType.NewMatch, recipeId), Times.Once);
    }

    [Fact]
    public async Task RecalculateGroupMatchesAsync_DeletesOldMatchesAndRecordsNew()
    {
        var groupId = Guid.NewGuid();
        var group = new Group
        {
            Id = groupId, Name = "Test",
            Members = new List<GroupMember> { new() { GroupId = groupId, UserId = "user-1", Role = GroupRole.Member } }
        };
        var recipeId1 = Guid.NewGuid();
        var recipeId2 = Guid.NewGuid();

        _likeRepo.Setup(x => x.GetCommonLikedRecipeIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Guid> { recipeId1, recipeId2 });

        await _sut.RecalculateGroupMatchesAsync(group);

        _likeRepo.Verify(x => x.DeleteMatchesByGroupIdAsync(groupId), Times.Once);
        _likeRepo.Verify(x => x.RecordMatchAsync(It.Is<MatchEntity>(m => m.GroupId == groupId && m.RecipeId == recipeId1)), Times.Once);
        _likeRepo.Verify(x => x.RecordMatchAsync(It.Is<MatchEntity>(m => m.GroupId == groupId && m.RecipeId == recipeId2)), Times.Once);
    }

    [Fact]
    public async Task GetLikedRecipesByUserIdAsync_ReturnsMappedDtos()
    {
        var recipeId = Guid.NewGuid();
        var recipe = new Recipe { Id = recipeId, Name = "TestOpskrift", Category = "Aftensmad", TotalTime = "30m" };
        _likeRepo.Setup(x => x.GetLikedRecipesByUserIdAsync("user-1")).ReturnsAsync(new List<Recipe> { recipe });

        var result = await _sut.GetLikedRecipesByUserIdAsync("user-1");

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("TestOpskrift");
        result.First().Category.Should().Be("Aftensmad");
        result.First().TotalTime.Should().Be("30m");
    }

    [Fact]
    public async Task RecordGroupMatch_WhenGroupNotFound_DoesNothing()
    {
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);

        await _sut.RecordGroupMatch(Guid.NewGuid(), Guid.NewGuid());

        _likeRepo.Verify(x => x.RecordMatchAsync(It.IsAny<MatchEntity>()), Times.Never);
        _notificationDispatcher.Verify(x => x.DispatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RecordGroupMatch_WhenGroupFound_RecordsMatchAndSendsNotifications()
    {
        var groupId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var group = new Group
        {
            Id = groupId, Name = "Familie",
            Members = new List<GroupMember>
            {
                new() { GroupId = groupId, UserId = "user-1", Role = GroupRole.Member }
            }
        };
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(groupId)).ReturnsAsync(group);

        await _sut.RecordGroupMatch(groupId, recipeId);

        _likeRepo.Verify(x => x.RecordMatchAsync(It.Is<MatchEntity>(m => m.GroupId == groupId && m.RecipeId == recipeId)), Times.Once);
        _notificationDispatcher.Verify(x => x.DispatchAsync("user-1", "Nyt match!", It.IsAny<string>(), NotificationType.NewMatch, recipeId), Times.Once);
    }
}
