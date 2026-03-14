using Vela.Application.Common;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class MealPlanService(IMealPlanRepository mealPlanRepository, IRecipeRepository recipeRepository) : IMealPlanService
{
    private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
    private readonly IRecipeRepository _recipeRepository  = recipeRepository;

    public async Task<Result<MealPlanDto>> GetMealPlanAsync(string? userId, Guid? groupId)
    {
        var hasUserId = !string.IsNullOrWhiteSpace(userId);
        var hasGroupId = groupId.HasValue && groupId != Guid.Empty;
        
        if (hasUserId == hasGroupId)
            return Result<MealPlanDto>.Fail("Mealplan must belong to either a user or a group");

        var mealPlan = new MealPlan();

        if (hasUserId)
        {
            mealPlan = await _mealPlanRepository.GetByUserIdAsync(userId);
            if (mealPlan == null)
                return Result<MealPlanDto>.Fail($"Meal plan with userID {userId} not found");
        }
        else
        {
            Guid foundgroupId = groupId ??Guid.Empty;
            mealPlan = await _mealPlanRepository.GetByGroupIdAsync(foundgroupId);
            if (mealPlan == null)
                return Result<MealPlanDto>.Fail($"Meal plan with groupId {groupId} not found");
        }

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

    public async Task<Result> UpdateMealPlanAsync(Guid mealPlanId, string name, string? description)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        mealPlan.Name = name;
        mealPlan.Description = description;
        mealPlan.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteMealPlanAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        await _mealPlanRepository.DeleteAsync(mealPlanId);
        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<MealPlanEntryDto>> AddRecipeToMealPlanAsync(Guid mealPlanId, AddMealPlanEntryRequest request)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result<MealPlanEntryDto>.Fail($"Meal plan with ID {mealPlanId} not found");

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

    public async Task<Result> RemoveRecipeFromMealPlanAsync(Guid mealPlanId, Guid entryId)
    {
        var entry = await _mealPlanRepository.GetEntryAsync(entryId);
        if (entry == null)
            return Result.Fail($"Meal plan entry with ID {entryId} not found");

        if (entry.MealPlanId != mealPlanId)
            return Result.Fail("Entry does not belong to this meal plan");

        await _mealPlanRepository.RemoveEntryAsync(entryId);
        await _mealPlanRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> UpdateMealPlanEntryServingsAsync(Guid mealPlanId, Guid entryId, int servings)
    {
        var entry = await _mealPlanRepository.GetEntryAsync(entryId);
        if (entry == null)
            return Result.Fail($"Meal plan entry with ID {entryId} not found");

        entry.Servings = servings;

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
            Recipe = recipeDto
        };
    }
}