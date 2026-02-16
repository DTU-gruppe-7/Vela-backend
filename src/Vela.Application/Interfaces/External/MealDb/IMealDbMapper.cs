using Vela.Application.DTOs;
using Vela.Domain.Entities;
namespace Vela.Application.Interfaces.External.MealDb;

public interface IMealDbMapper
{
    Recipe MapToRecipe(MealDto mealDto);
    IEnumerable<(string Name, string Measure)> ExtractIngredients(MealDto mealDto);
}