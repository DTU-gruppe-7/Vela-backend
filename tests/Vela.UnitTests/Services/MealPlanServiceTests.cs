using FluentAssertions;
using Moq;
using Vela.Application.Common;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Application.Services;
using Vela.Domain.Entities.Group;
using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class MealPlanServiceTests
{
    private readonly Mock<IMealPlanRepository> _mealPlanRepo = new();
    private readonly Mock<IRecipeRepository> _recipeRepo = new();
    private readonly Mock<IShoppingListRepository> _shoppingListRepo = new();
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<IGroupAuthorizationService> _groupAuth = new();
    private readonly MealPlanService _sut;

    public MealPlanServiceTests()
    {
        _sut = new MealPlanService(
            _mealPlanRepo.Object,
            _recipeRepo.Object,
            _shoppingListRepo.Object,
            _groupRepo.Object,
            _groupAuth.Object);
    }

    // ───────────────────── helpers ─────────────────────

    private static MealPlan CreatePersonalMealPlan(string userId = "user-1") => new()
    {
        Id = Guid.NewGuid(), UserId = userId, Name = "Min madplan"
    };

    private static MealPlan CreateGroupMealPlan(Guid groupId) => new()
    {
        Id = Guid.NewGuid(), GroupId = groupId, Name = "Gruppe madplan"
    };

    private static Group CreateGroupWithMember(Guid groupId, string userId) => new()
    {
        Id = groupId, Name = "Test",
        Members = new List<GroupMember> { new() { GroupId = groupId, UserId = userId, Role = GroupRole.Member } }
    };

    private static Recipe CreateRecipe() => new()
    {
        Id = Guid.NewGuid(), Name = "Pasta Carbonara", ServingSize = 4
    };

    // ───────────────────── CreateMealPlanAsync ─────────────────────

    [Fact]
    public async Task CreateMealPlanAsync_WithUserIdOnly_ReturnsOk()
    {
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        var result = await _sut.CreateMealPlanAsync("user-1", null, "Min madplan");
        result.Success.Should().BeTrue();
        result.Data!.UserId.Should().Be("user-1");
        result.Data.Name.Should().Be("Min madplan");
    }

    [Fact]
    public async Task CreateMealPlanAsync_WithGroupIdOnly_ReturnsOk()
    {
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        var groupId = Guid.NewGuid();
        var result = await _sut.CreateMealPlanAsync(null, groupId, "Gruppe plan");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateMealPlanAsync_WithBothUserIdAndGroupId_ReturnsFail()
    {
        var result = await _sut.CreateMealPlanAsync("user-1", Guid.NewGuid(), "Test");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("either a user or a group");
    }

    [Fact]
    public async Task CreateMealPlanAsync_WithNeitherUserIdNorGroupId_ReturnsFail()
    {
        var result = await _sut.CreateMealPlanAsync(null, null, "Test");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("either a user or a group");
    }

    // ───────────────────── GetMealPlanAsync ─────────────────────

    [Fact]
    public async Task GetMealPlanAsync_PersonalPlan_WhenExists_ReturnsOk()
    {
        var mp = CreatePersonalMealPlan();
        _mealPlanRepo.Setup(x => x.GetByUserIdAsync("user-1")).ReturnsAsync(mp);
        _mealPlanRepo.Setup(x => x.GetByIdWithEntriesByDateRangeAsync(mp.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(mp);

        var result = await _sut.GetMealPlanAsync("user-1", null, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "user-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetMealPlanAsync_PersonalPlan_WhenNotFound_ReturnsFail()
    {
        _mealPlanRepo.Setup(x => x.GetByUserIdAsync("user-1")).ReturnsAsync((MealPlan?)null);

        var result = await _sut.GetMealPlanAsync("user-1", null, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "user-1");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task GetMealPlanAsync_GroupPlan_WhenCallerIsMember_ReturnsOk()
    {
        var groupId = Guid.NewGuid();
        var mp = CreateGroupMealPlan(groupId);
        var group = CreateGroupWithMember(groupId, "user-1");

        _mealPlanRepo.Setup(x => x.GetByGroupIdAsync(groupId)).ReturnsAsync(mp);
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(groupId)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeMembership(group, "user-1")).Returns(Result.Ok());
        _mealPlanRepo.Setup(x => x.GetByIdWithEntriesByDateRangeAsync(mp.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(mp);

        var result = await _sut.GetMealPlanAsync(null, groupId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "user-1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetMealPlanAsync_GroupPlan_WhenCallerNotMember_ReturnsForbidden()
    {
        var groupId = Guid.NewGuid();
        var mp = CreateGroupMealPlan(groupId);
        var group = CreateGroupWithMember(groupId, "other-user");

        _mealPlanRepo.Setup(x => x.GetByGroupIdAsync(groupId)).ReturnsAsync(mp);
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(groupId)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeMembership(group, "stranger")).Returns(Result.Fail("Not a member", ResultErrorType.Forbidden));

        var result = await _sut.GetMealPlanAsync(null, groupId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "stranger");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Forbidden);
    }

    [Fact]
    public async Task GetMealPlanAsync_WithBothParams_ReturnsFail()
    {
        var result = await _sut.GetMealPlanAsync("user-1", Guid.NewGuid(), new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "user-1");
        result.Success.Should().BeFalse();
    }

    // ───────────────────── UpdateMealPlanAsync ─────────────────────

    [Fact]
    public async Task UpdateMealPlanAsync_WhenAuthorized_UpdatesAndReturnsOk()
    {
        var mp = CreatePersonalMealPlan();
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.UpdateMealPlanAsync(mp.Id, "Ny Navn", "Ny Beskrivelse", "user-1");

        result.Success.Should().BeTrue();
        mp.Name.Should().Be("Ny Navn");
        mp.Description.Should().Be("Ny Beskrivelse");
    }

    [Fact]
    public async Task UpdateMealPlanAsync_WhenNotFound_ReturnsFail()
    {
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid?>())).ReturnsAsync((MealPlan?)null);

        var result = await _sut.UpdateMealPlanAsync(Guid.NewGuid(), "Test", null, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateMealPlanAsync_WhenForbidden_ReturnsFail()
    {
        var groupId = Guid.NewGuid();
        var mp = CreateGroupMealPlan(groupId);
        var group = CreateGroupWithMember(groupId, "other");

        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _groupRepo.Setup(x => x.GetGroupWithMembersAsync(groupId)).ReturnsAsync(group);
        _groupAuth.Setup(x => x.AuthorizeMembership(group, "stranger")).Returns(Result.Fail("Nope", ResultErrorType.Forbidden));

        var result = await _sut.UpdateMealPlanAsync(mp.Id, "Hack", null, "stranger");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Forbidden);
    }

    // ───────────────────── DeleteMealPlanAsync ─────────────────────

    [Fact]
    public async Task DeleteMealPlanAsync_WhenAuthorized_DeletesAndReturnsOk()
    {
        var mp = CreatePersonalMealPlan();
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.DeleteMealPlanAsync(mp.Id, "user-1");

        result.Success.Should().BeTrue();
        _mealPlanRepo.Verify(x => x.DeleteAsync(mp), Times.Once);
    }

    [Fact]
    public async Task DeleteMealPlanAsync_WhenNotFound_ReturnsFail()
    {
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid?>())).ReturnsAsync((MealPlan?)null);

        var result = await _sut.DeleteMealPlanAsync(Guid.NewGuid(), "user-1");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    // ───────────────────── AddRecipeToMealPlanAsync ─────────────────────

    [Fact]
    public async Task AddRecipeToMealPlanAsync_WhenValid_ReturnsCreatedEntry()
    {
        var mp = CreatePersonalMealPlan();
        var recipe = CreateRecipe();
        var request = new AddMealPlanEntryRequest { RecipeId = recipe.Id, Date = new DateOnly(2026, 4, 15), MealType = "Dinner", Servings = 4 };

        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _recipeRepo.Setup(x => x.GetByUuidAsync(recipe.Id)).ReturnsAsync(recipe);
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.AddRecipeToMealPlanAsync(mp.Id, request, "user-1");

        result.Success.Should().BeTrue();
        result.Data!.RecipeId.Should().Be(recipe.Id);
        result.Data.MealType.Should().Be("Dinner");
    }

    [Fact]
    public async Task AddRecipeToMealPlanAsync_WhenMealPlanNotFound_ReturnsFail()
    {
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid?>())).ReturnsAsync((MealPlan?)null);
        var request = new AddMealPlanEntryRequest { RecipeId = Guid.NewGuid(), Date = new DateOnly(2026, 4, 15), MealType = "Dinner" };

        var result = await _sut.AddRecipeToMealPlanAsync(Guid.NewGuid(), request, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task AddRecipeToMealPlanAsync_WhenRecipeNotFound_ReturnsFail()
    {
        var mp = CreatePersonalMealPlan();
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _recipeRepo.Setup(x => x.GetByUuidAsync(It.IsAny<Guid?>())).ReturnsAsync((Recipe?)null);
        var request = new AddMealPlanEntryRequest { RecipeId = Guid.NewGuid(), Date = new DateOnly(2026, 4, 15), MealType = "Dinner" };

        var result = await _sut.AddRecipeToMealPlanAsync(mp.Id, request, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    // ───────────────────── RemoveRecipeFromMealPlanAsync ─────────────────────

    [Fact]
    public async Task RemoveRecipeFromMealPlanAsync_WhenValid_RemovesAndReturnsOk()
    {
        var mp = CreatePersonalMealPlan();
        var entry = new MealPlanEntry
        {
            Id = Guid.NewGuid(), MealPlanId = mp.Id, MealPlan = mp,
            RecipeId = Guid.NewGuid(), Recipe = CreateRecipe(),
            Date = new DateOnly(2026, 4, 10), MealType = "Dinner"
        };

        _mealPlanRepo.Setup(x => x.GetEntryAsync(entry.Id)).ReturnsAsync(entry);
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.RemoveRecipeFromMealPlanAsync(mp.Id, entry.Id, "user-1");

        result.Success.Should().BeTrue();
        _mealPlanRepo.Verify(x => x.RemoveEntryAsync(entry.Id), Times.Once);
    }

    [Fact]
    public async Task RemoveRecipeFromMealPlanAsync_WhenEntryBelongsToDifferentPlan_ReturnsFail()
    {
        var entry = new MealPlanEntry
        {
            Id = Guid.NewGuid(), MealPlanId = Guid.NewGuid(),
            MealPlan = CreatePersonalMealPlan(),
            RecipeId = Guid.NewGuid(), Recipe = CreateRecipe(),
            Date = new DateOnly(2026, 4, 10), MealType = "Dinner"
        };
        _mealPlanRepo.Setup(x => x.GetEntryAsync(entry.Id)).ReturnsAsync(entry);

        var result = await _sut.RemoveRecipeFromMealPlanAsync(Guid.NewGuid(), entry.Id, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not belong");
    }

    // ───────────────────── UpdateMealPlanEntryServingsAsync ─────────────────────

    [Fact]
    public async Task UpdateMealPlanEntryServingsAsync_WhenValid_UpdatesServingsAndDate()
    {
        var mp = CreatePersonalMealPlan();
        var entry = new MealPlanEntry
        {
            Id = Guid.NewGuid(), MealPlanId = mp.Id, MealPlan = mp,
            RecipeId = Guid.NewGuid(), Recipe = CreateRecipe(),
            Date = new DateOnly(2026, 4, 10), MealType = "Dinner", Servings = 2
        };

        _mealPlanRepo.Setup(x => x.GetEntryAsync(entry.Id)).ReturnsAsync(entry);
        _mealPlanRepo.Setup(x => x.GetByUuidAsync(mp.Id)).ReturnsAsync(mp);
        _mealPlanRepo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var newDate = new DateOnly(2026, 5, 1);
        var result = await _sut.UpdateMealPlanEntryServingsAsync(mp.Id, entry.Id, 6, newDate, "user-1");

        result.Success.Should().BeTrue();
        entry.Servings.Should().Be(6);
        entry.Date.Should().Be(newDate);
    }

    // ───────────────────── GetAggregatedMealPlanAsync ─────────────────────

    [Fact]
    public async Task GetAggregatedMealPlanAsync_CombinesPersonalAndGroupEntries()
    {
        var userId = "user-1";
        var personalMp = CreatePersonalMealPlan(userId);
        var recipe = CreateRecipe();
        var personalEntry = new MealPlanEntry
        {
            Id = Guid.NewGuid(), MealPlanId = personalMp.Id, MealPlan = personalMp,
            RecipeId = recipe.Id, Recipe = recipe,
            Date = new DateOnly(2026, 4, 10), MealType = "Dinner"
        };
        personalMp.Entries.Add(personalEntry);

        var groupId = Guid.NewGuid();
        var groupMp = CreateGroupMealPlan(groupId);
        var groupEntry = new MealPlanEntry
        {
            Id = Guid.NewGuid(), MealPlanId = groupMp.Id, MealPlan = groupMp,
            RecipeId = recipe.Id, Recipe = recipe,
            Date = new DateOnly(2026, 4, 12), MealType = "Lunch"
        };
        groupMp.Entries.Add(groupEntry);

        var group = new Group { Id = groupId, Name = "Familie" };

        _mealPlanRepo.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(personalMp);
        _mealPlanRepo.Setup(x => x.GetByIdWithEntriesByDateRangeAsync(personalMp.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(personalMp);
        _groupRepo.Setup(x => x.GetGroupsByUserIdAsync(userId)).ReturnsAsync(new List<Group?> { group });
        _mealPlanRepo.Setup(x => x.GetAllGroupMealPlans(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<MealPlan> { groupMp });
        _mealPlanRepo.Setup(x => x.GetByIdWithEntriesByDateRangeAsync(groupMp.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(groupMp);

        var result = await _sut.GetAggregatedMealPlanAsync(userId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30));

        result.Success.Should().BeTrue();
        result.Data!.Entries.Should().HaveCount(2);
        result.Data.Entries.Should().Contain(e => e.Source == "personal");
        result.Data.Entries.Should().Contain(e => e.Source == "group" && e.SourceGroupName == "Familie");
    }
}
