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

    public async Task<Result<MealPlanDto>> GetMealPlanAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result<MealPlanDto>.Fail($"Meal plan with ID {mealPlanId} not found");

        return Result<MealPlanDto>.Ok(MapToDto(mealPlan, new List<MealPlanEntry>()));
    }

    public async Task<Result<IEnumerable<MealPlanDto>>> GetAllMealPlansAsync()
    {
        var mealPlans = await _mealPlanRepository.GetAllAsync();
        var dtos = mealPlans.Select(mp => MapToDto(mp, mp.Entries)).ToList();
        return Result<IEnumerable<MealPlanDto>>.Ok(dtos);
    }

    public async Task<Result<IEnumerable<MealPlanDto>>> GetAllMealPlansByUserAsync(string userId)
    {
        var mealPlans = await _mealPlanRepository.GetByUserIdAsync(userId);
        var dtos = mealPlans.Select(mp => MapToDto(mp, mp.Entries)).ToList();
        return Result<IEnumerable<MealPlanDto>>.Ok(dtos);
    }

    public async Task<Result<MealPlanDto>> CreateMealPlanAsync(string userId, string name, string? description = null)
    {
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _mealPlanRepository.AddAsync(mealPlan);
        await _mealPlanRepository.SaveChangesAsync();
        return Result<MealPlanDto>.Ok(MapToDto(mealPlan, new List<MealPlanEntry>()));
    }

    public async Task<Result> UpdateMealPlanAsync(Guid mealPlanId, string name, string? description)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            return Result.Fail($"Meal plan with ID {mealPlanId} not found");

        mealPlan.Name = name;
        mealPlan.Description = description;
        mealPlan.UpdatedAt = DateTimeOffset.UtcNow;

        await _mealPlanRepository.UpdateAsync(mealPlan);
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

    public async Task<Result<MealPlanDto>> GetMealPlanWithEntriesAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetMealPlanWithEntriesAsync(mealPlanId);
        if (mealPlan == null)
            return Result<MealPlanDto>.Fail($"Meal plan with ID {mealPlanId} not found");

        return Result<MealPlanDto>.Ok(MapToDto(mealPlan, mealPlan.Entries));
    }

    private MealPlanDto MapToDto(MealPlan mealPlan, List<MealPlanEntry> entries)
    {
        return new MealPlanDto
        {
            Id = mealPlan.Id,
            UserId = mealPlan.UserId,
            Name = mealPlan.Name,
            Description = mealPlan.Description,
            CreatedAt = mealPlan.CreatedAt,
            UpdatedAt = mealPlan.UpdatedAt,
            Entries = entries.Select(MapEntryToDto).ToList()
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