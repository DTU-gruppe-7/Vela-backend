using Vela.Application.DTOs;
using Vela.Application.Interfaces.External.MealDb;
using Vela.Domain.Entities;

namespace Vela.Infrastructure.External.MealDb;

public class MealDbMapper : IMealDbMapper
{
    public Recipe MapToRecipe(MealDto mealDto)
    {
        return new Recipe
        {
            Id = Guid.NewGuid(),
            ExternalId = mealDto.IdMeal,
            Name = mealDto.StrMeal,
            Instructions = mealDto.StrInstructions,
            Category = mealDto.StrCategory,
            ThumbnailUrl = mealDto.StrMealThumb,
            Ingredients = new List<RecipeIngredient>()
        };
    }

    public IEnumerable<(string Name, string Measure)> ExtractIngredients(MealDto mealDto)
    {
        var ingredients = new List<(string name, String Measure)>();

        for (int i = 1; i <= 20; i++)
        {
            string name = GetPropertyValue(mealDto, $"StrIngredient{i}");
            string measure = GetPropertyValue(mealDto, $"StrMeasure{i}");

            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add((name, measure));
            }
        }
        return ingredients;
    }

    private string GetPropertyValue(object src, string propertyName)
    {
        return src.GetType().GetProperty(propertyName)?.GetValue(src, null)?.ToString() ?? "";
    }
}