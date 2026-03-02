using Vela.Application.DTOs.MealPlan;
using Vela.Application.DTOs.Recipe;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class MealPlanService : IMealPlanService
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IRecipeRepository _recipeRepository;

    public MealPlanService(IMealPlanRepository mealPlanRepository, IRecipeRepository recipeRepository)
    {
        _mealPlanRepository = mealPlanRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<MealPlanDto?> GetMealPlanAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null) return null;

        return MapToDto(mealPlan, new List<MealPlanEntry>());
    }

    public async Task<IEnumerable<MealPlanDto>> GetAllMealPlansAsync()
    {
        var mealPlans = await _mealPlanRepository.GetAllAsync();
        return mealPlans.Select(mp => MapToDto(mp, mp.Entries)).ToList();
    }

    public async Task<MealPlanDto> CreateMealPlanAsync(string name, string? description = null)
    {
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mealPlanRepository.AddAsync(mealPlan);
        return MapToDto(mealPlan, new List<MealPlanEntry>());
    }

    public async Task UpdateMealPlanAsync(Guid mealPlanId, string name, string? description)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            throw new KeyNotFoundException($"Meal plan with ID {mealPlanId} not found");

        mealPlan.Name = name;
        mealPlan.Description = description;
        mealPlan.UpdatedAt = DateTime.UtcNow;

        await _mealPlanRepository.UpdateAsync(mealPlan);
    }

    public async Task DeleteMealPlanAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            throw new KeyNotFoundException($"Meal plan with ID {mealPlanId} not found");

        await _mealPlanRepository.DeleteAsync(mealPlanId);
    }

    public async Task<MealPlanEntryDto> AddRecipeToMealPlanAsync(Guid mealPlanId, AddMealPlanEntryRequest request)
    {
        var mealPlan = await _mealPlanRepository.GetByUuidAsync(mealPlanId);
        if (mealPlan == null)
            throw new KeyNotFoundException($"Meal plan with ID {mealPlanId} not found");

        var recipe = await _recipeRepository.GetByUuidAsync(request.RecipeId);
        if (recipe == null)
            throw new KeyNotFoundException($"Recipe with ID {request.RecipeId} not found");

        var entry = new MealPlanEntry
        {
            Id = Guid.NewGuid(),
            MealPlanId = mealPlanId,
            MealPlan = mealPlan,
            RecipeId = request.RecipeId,
            Recipe = recipe,
            Day = request.Day,
            MealType = request.MealType,
            Servings = request.Servings,
            AddedAt = DateTime.UtcNow
        };

        await _mealPlanRepository.AddEntryAsync(entry);

        return MapEntryToDto(entry);
    }

    public async Task RemoveRecipeFromMealPlanAsync(Guid mealPlanId, Guid entryId)
    {
        var entry = await _mealPlanRepository.GetEntryAsync(entryId);
        if (entry == null)
            throw new KeyNotFoundException($"Meal plan entry with ID {entryId} not found");

        if (entry.MealPlanId != mealPlanId)
            throw new InvalidOperationException("Entry does not belong to this meal plan");

        await _mealPlanRepository.RemoveEntryAsync(entryId);
    }

    public async Task<MealPlanDto?> GetMealPlanWithEntriesAsync(Guid mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetMealPlanWithEntriesAsync(mealPlanId);
        if (mealPlan == null) return null;

        return MapToDto(mealPlan, mealPlan.Entries);
    }

    private MealPlanDto MapToDto(MealPlan mealPlan, List<MealPlanEntry> entries)
    {
        return new MealPlanDto
        {
            Id = mealPlan.Id,
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
            Day = entry.Day,
            MealType = entry.MealType,
            Servings = entry.Servings,
            AddedAt = entry.AddedAt,
            Recipe = recipeDto
        };
    }
}
