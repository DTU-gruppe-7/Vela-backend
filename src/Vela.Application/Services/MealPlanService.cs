using System.Xml.Serialization;
using Vela.Application.Common;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities.Group;
using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Entities.Recipes;

namespace Vela.Application.Services;

public class MealPlanService(
    IMealPlanRepository mealPlanRepository,
    IRecipeRepository recipeRepository,
    IShoppingListRepository shoppingListRepository,
    IGroupRepository groupRepository,
    IGroupAuthorizationService groupAuthorizationService) : IMealPlanService
{
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    private readonly IRecipeRepository _recipeRepository  = recipeRepository;
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IGroupAuthorizationService _groupAuthorizationService = groupAuthorizationService;

    private async Task<Result> AuthorizeMealPlanAccessAsync(MealPlan mealPlan, string callerUserId)
    {
        if (mealPlan.GroupId == null) return Result.Ok();
        var group = await _groupRepository.GetGroupWithMembersAsync(mealPlan.GroupId.Value);
        if (group == null) return Result.Fail("Group not found", ResultErrorType.NotFound);
        return _groupAuthorizationService.AuthorizeMembership(group, callerUserId);
    }

    public async Task<Result<MealPlanDto>> GetMealPlanAsync(string? userId, Guid? groupId, DateOnly startDate, DateOnly endDate, string callerUserId)
    {
        var hasUserId = !string.IsNullOrWhiteSpace(userId);
        var hasGroupId = groupId.HasValue && groupId != Guid.Empty;

        if (hasUserId == hasGroupId)
            return Result<MealPlanDto>.Fail("Mealplan must belong to either a user or a group");

        MealPlan mealPlan;

        if (hasUserId)
        {
            mealPlan = await _mealPlanRepository.GetByUserIdAsync(userId);
            if (mealPlan == null)
                return Result<MealPlanDto>.Fail($"Meal plan with userID {userId} not found");
        }
        else
        {
            Guid foundgroupId = groupId ?? Guid.Empty;
            mealPlan = await _mealPlanRepository.GetByGroupIdAsync(foundgroupId);
            if (mealPlan == null)
                return Result<MealPlanDto>.Fail($"Meal plan with groupId {groupId} not found");

            var authResult = await AuthorizeMealPlanAccessAsync(mealPlan, callerUserId);
            if (!authResult.Success)
                return Result<MealPlanDto>.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);
        }

        mealPlan = await _mealPlanRepository.GetByIdWithEntriesByDateRangeAsync(mealPlan.Id, startDate, endDate);
        
        return Result<MealPlanDto>.Ok(MapToDto(mealPlan));
    }
    
    public async Task<Result<MealPlanDto>> CreateMealPlanAsync(string? userId, Guid? groupId, string name)
    {
        var hasUserId = !string.IsNullOrWhiteSpace(userId);
        var hasGroupId = groupId.HasValue && groupId != Guid.Empty;
        
        if (hasUserId == hasGroupId)
            return Result<MealPlanDto>.Fail("Mealplan must belong to either a user or a group. Not both or none");
        
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = groupId,
            Name = name,
            Description = String.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _mealPlanRepository.AddAsync(mealPlan);
        await _mealPlanRepository.SaveChangesAsync();
        return Result<MealPlanDto>.Ok(MapToDto(mealPlan));
    }

    public async Task<Result> UpdateMealPlanAsync(Guid mealPlanId, string name, string? description, string callerUserId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        var authResult = await AuthorizeMealPlanAccessAsync(mealPlan, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        mealPlan.Name = name;
        mealPlan.Description = description;
        mealPlan.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteMealPlanAsync(Guid mealPlanId, string callerUserId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        var authResult = await AuthorizeMealPlanAccessAsync(mealPlan, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        await _mealPlanRepository.DeleteAsync(mealPlan);
        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<MealPlanEntryDto>> AddRecipeToMealPlanAsync(Guid mealPlanId, AddMealPlanEntryRequest request, string callerUserId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result<MealPlanEntryDto>.Fail($"Meal plan with ID {mealPlanId} not found");

        var authResult = await AuthorizeMealPlanAccessAsync(mealPlan, callerUserId);
        if (!authResult.Success)
            return Result<MealPlanEntryDto>.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        var recipe = await _recipeRepository.GetByUuidAsync(request.RecipeId);
        if (recipe == null)
            return Result<MealPlanEntryDto>.Fail($"Recipe with ID {request.RecipeId} not found");

        var entry = new MealPlanEntry
        {
            Id = Guid.NewGuid(),
            MealPlanId = mealPlanId,
            MealPlan = mealPlan,
            RecipeId = request.RecipeId,
            Recipe = recipe,
            Date = request.Date,
            MealType = request.MealType,
            Servings = request.Servings,
            AddedAt = DateTimeOffset.UtcNow
        };
        
        mealPlan.Entries.Add(entry);

        await _mealPlanRepository.AddEntryAsync(entry);
        await _mealPlanRepository.SaveChangesAsync();
        return Result<MealPlanEntryDto>.Ok(MapEntryToDto(entry));
    }

    public async Task<Result> RemoveRecipeFromMealPlanAsync(Guid mealPlanId, Guid entryId, string callerUserId)
    {
        var entry = await _mealPlanRepository.GetEntryAsync(entryId);
        if (entry == null)
            return Result.Fail($"Meal plan entry with ID {entryId} not found");

        if (entry.MealPlanId != mealPlanId)
            return Result.Fail("Entry does not belong to this meal plan");

        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        var authResult = await AuthorizeMealPlanAccessAsync(mealPlan, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        await _shoppingListRepository.DeleteItemsByMealPlanEntryIdAsync(entryId);

        await _mealPlanRepository.RemoveEntryAsync(entryId);
        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> UpdateMealPlanEntryServingsAsync(Guid mealPlanId, Guid entryId, int? servings, DateOnly? newdate, string callerUserId)
    {
        var entry = await _mealPlanRepository.GetEntryAsync(entryId);
        if (entry == null)
            return Result.Fail($"Meal plan entry with ID {entryId} not found");

        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        var authResult = await AuthorizeMealPlanAccessAsync(mealPlan, callerUserId);
        if (!authResult.Success)
            return Result.Fail(authResult.ErrorMessage!, ResultErrorType.Forbidden);

        if (servings.HasValue)
        {
            entry.Servings = servings.Value;
        }

        if (newdate.HasValue)
        {
            entry.Date = newdate.Value;
        }

        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<MealPlanDto>> GetMealPlanWithEntriesAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetMealPlanWithEntriesAsync(mealPlanId);
        if (mealPlan == null)
            return Result<MealPlanDto>.Fail($"Meal plan with ID {mealPlanId} not found");

        return Result<MealPlanDto>.Ok(MapToDto(mealPlan));
    }

    public async Task<Result<MealPlanDto>> GetAggregatedMealPlanAsync(string userId, DateOnly startDate, DateOnly endDate)
    {
        var aggregatedEntries = new List<MealPlanEntryDto>();
        
        // 1. hent personlige mealplan
        var personalMealPlan = await _mealPlanRepository.GetByUserIdAsync(userId);
        if (personalMealPlan != null)
        {
            var selectedPersonalMealPlan = await _mealPlanRepository.GetByIdWithEntriesByDateRangeAsync(personalMealPlan.Id, startDate, endDate);
            if (selectedPersonalMealPlan != null)
            {
                var personalEntries = selectedPersonalMealPlan.Entries.Select(e => { var dto = MapEntryToDto(e);
                    dto.Source = "personal";
                    return dto;
                });
                aggregatedEntries.AddRange(personalEntries);
            }
        }
        // 2. Find brugerens grupper (for at få ID'er og Navne)
        var groups = await _groupRepository.GetGroupsByUserIdAsync(userId);
        var validGroups = groups.Where(g => g != null).Select(g => g!).ToList();
        var groupIds = validGroups.Select(g => g.Id);
        var groupDict = validGroups.ToDictionary(g => g.Id, g => g.Name);
        
        // 3. Batch-hent alle gruppe-madplaner
        if (groupIds.Any())
        {
            var groupPlans = await _mealPlanRepository.GetAllGroupMealPlans(groupIds);
            foreach (var plan in groupPlans)
            {
                var groupName = plan.GroupId.HasValue && groupDict.TryGetValue(plan.GroupId.Value, out var name) ? name : "Ukendt gruppe";
                var selectedMealPlan = await _mealPlanRepository.GetByIdWithEntriesByDateRangeAsync(plan.Id, startDate, endDate);
                if (selectedMealPlan != null)
                {
                    var groupEntries = selectedMealPlan.Entries.Select(e => {
                        var dto = MapEntryToDto(e);
                        dto.Source = "group";
                        dto.SourceGroupName = groupName;
                        return dto;
                    });
                    aggregatedEntries.AddRange(groupEntries);
                }
            }
        }
        var mealPlanDto = MapToDto(personalMealPlan);
        mealPlanDto.Entries = aggregatedEntries;
        return Result<MealPlanDto>.Ok(mealPlanDto);
    }   
    private MealPlanDto MapToDto(MealPlan mealPlan)
    {
        return new MealPlanDto
        {
            Id = mealPlan.Id,
            UserId = mealPlan.UserId,
            Name = mealPlan.Name,
            Description = mealPlan.Description,
            CreatedAt = mealPlan.CreatedAt,
            UpdatedAt = mealPlan.UpdatedAt,
            Entries = mealPlan.Entries.Select(MapEntryToDto).ToList()
        };
    }

    private MealPlanEntryDto MapEntryToDto(MealPlanEntry entry)
    {
        RecipeSummaryDto? recipeDto = null;
        if (entry.Recipe != null)
        {
            recipeDto = new RecipeSummaryDto
            {
                Id = entry.Recipe.Id,
                Name = entry.Recipe.Name,
                Category = entry.Recipe.Category,
                ThumbnailUrl = entry.Recipe.ThumbnailUrl,
                WorkTime = entry.Recipe.WorkTime,
                TotalTime = entry.Recipe.TotalTime,
                KeywordsJson = entry.Recipe.KeywordsJson
            };
        }
        return new MealPlanEntryDto
        {
            Id = entry.Id,
            MealPlanId = entry.MealPlanId,
            RecipeId = entry.RecipeId,
            Date = entry.Date,
            MealType = entry.MealType,
            Servings = entry.Servings,
            AddedAt = entry.AddedAt,
            Recipe = recipeDto,
            AddedToShoppingList = entry.AddedToShoppingList
        };
    }
}