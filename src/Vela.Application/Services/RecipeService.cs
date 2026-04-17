using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;

namespace Vela.Application.Services;

public class RecipeService(IRecipeRepository recipeRepository) : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository;
    
    public async Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync(IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false)
    {
        var recipes = await _recipeRepository.GetAllSummariesAsync(excludedAllergens, requireVeganRecipes);

        return recipes.Select(r => FromEntity(r)).ToList();
    }

    public async Task<RecipeDto?> GetRecipeByIdAsync(Guid recipeId)
    {
        var recipe = await _recipeRepository.GetByIdWithIngredientsAsync(recipeId);
        
        if (recipe == null)
            return null;

        return new RecipeDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Category = recipe.Category,
            ThumbnailUrl = recipe.ThumbnailUrl,
            ServingSize = recipe.ServingSize,
            TotalTime = recipe.TotalTime,
            WorkTime = recipe.WorkTime,
            InstructionsJson = recipe.InstructionsJson,
            KeywordsJson = recipe.KeywordsJson,
            Ingredients = recipe.Ingredients.Select(ri => new RecipeIngredientDto
            {
                Id = ri.Id,
                IngredientId = ri.IngredientId,
                IngredientName = ri.Ingredient.Name,
                Quantity = ri.Quantity,
                Unit = ri.Unit,
                Section = ri.Section,
            }).ToList()
        };
    }

    public async Task<IEnumerable<RecipeSummaryDto>> GetNextRecipesAsync(string userId, int limit, string? category = null, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false)

    {
        var recipes = await _recipeRepository.GetNextRecipesAsync(userId, limit, category, excludedAllergens, requireVeganRecipes);

        return recipes.Select(r => FromEntity(r))
        .ToList();
    }
    
    private static RecipeSummaryDto FromEntity(Recipe r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Category = r.Category,
        ThumbnailUrl = r.ThumbnailUrl,
        WorkTime = r.WorkTime,
        TotalTime = r.TotalTime,
        KeywordsJson = r.KeywordsJson
    };
    
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _recipeRepository.GetCategoriesAsync();
    }

    public async Task<IEnumerable<RecipeSummaryDto>> GetMostLikedRecipesAsync(int limit = 20, IReadOnlyCollection<RecipeAllergen>? excludedAllergens = null, bool requireVeganRecipes = false)
    {
        var recipes = await _recipeRepository.GetMostLikedRecipesAsync(limit, excludedAllergens, requireVeganRecipes);
        var recipeList = recipes.ToList();

        return recipeList.Select(r => FromEntity(r)).ToList();
    }
}