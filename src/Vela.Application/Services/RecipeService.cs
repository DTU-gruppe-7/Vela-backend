using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Domain.Entities;

namespace Vela.Application.Services;

public class RecipeService(IRecipeRepository recipeRepository) : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository;
    
    public async Task<IEnumerable<RecipeSummaryDto>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository.GetAllSummariesAsync();

        return recipes.Select(r => new RecipeSummaryDto
        {
            Id = r.Id,
            Name = r.Name,
            Category = r.Category,
            ThumbnailUrl = r.ThumbnailUrl,
            WorkTime = r.WorkTime,
            TotalTime = r.TotalTime,
            KeywordsJson = r.KeywordsJson,
        });
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

    public async Task<IEnumerable<RecipeSummaryDto>> GetNextRecipesAsync(Guid userId, int limit)
    {
        var recipes = await _recipeRepository.GetNextRecipesAsync(userId, limit);

        return recipes.Select(r => new RecipeSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                Category = r.Category,
                ThumbnailUrl = r.ThumbnailUrl,
                WorkTime = r.WorkTime,
                TotalTime = r.TotalTime,
                KeywordsJson = r.KeywordsJson
            }
        )
        .ToList();
    }
}