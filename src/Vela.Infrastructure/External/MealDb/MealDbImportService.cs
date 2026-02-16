using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.External.MealDb;
using Vela.Domain.Entities;

namespace Vela.Infrastructure.External.MealDb;

public class MealDbImportService : IMealDbImportService
{
    private readonly IMealDbApiClient _apiClient;
    private readonly IMealDbMapper _mealMapper;
    private readonly IMeasureParser _measureParser;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IIngredientRepository _ingredientRepository;

    public MealDbImportService(
        IMealDbApiClient apiClient,
        IMealDbMapper mealMapper,
        IMeasureParser measureParser,
        IRecipeRepository recipeRepository,
        IIngredientRepository ingredientRepository)
    {
        _apiClient = apiClient;
        _mealMapper = mealMapper;
        _measureParser = measureParser;
        _recipeRepository = recipeRepository;
        _ingredientRepository = ingredientRepository;
    }

    public async Task ImportAllRecipesAsync()
    {
        foreach (char letter in "abcdefghijklmnopqrstuvwxyz")
        {
            await ImportByLetterAsync(letter.ToString());
        }
    }

    private async Task ImportByLetterAsync(string letter)
    {
        var response = await _apiClient.GetMealByLetterAsync(letter);

        if (response?.Meals == null)
            return;

        foreach (var mealDto in response.Meals)
        {
            if (await _recipeRepository.ExistsByExternalIdAsync(mealDto.IdMeal))
                continue;
            
            var recipe = _mealMapper.MapToRecipe(mealDto);
            await ProcessIngredientsAsync(recipe, mealDto);
            await _recipeRepository.AddAsync(recipe);
        }
    }

    private async Task ProcessIngredientsAsync(Recipe recipe, MealDto mealDto)
    {
        var processedIngredients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ingredientData = _mealMapper.ExtractIngredients(mealDto);

        foreach (var (name, measure) in ingredientData)
        {
            var ingredientKey = name.ToLower();

            if (processedIngredients.Contains(ingredientKey))
                continue;
            
            processedIngredients.Add(ingredientKey);

            var ingredient = await GetOrCreateIngredientAsync(name);
            var (quantity, unit) = _measureParser.Parse(measure);

            var recipeIngredient = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                Recipe = recipe,
                IngredientId = ingredient.Id,
                Ingredient = ingredient,
                RawMeasure = measure,
                Quantity = quantity,
                Unit = unit
            };
            
            recipe.Ingredients.Add(recipeIngredient);
        }
    }

    private async Task<Ingredient> GetOrCreateIngredientAsync(string name)
    {
        var existing = await _ingredientRepository.GetByNameAsync(name);

        if (existing != null)
            return existing;

        var newIngredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContainsGluten = false,
            ContainsLactose = false,
            ContainsNuts = false,
            IsVegan = false
        };
        
        await _ingredientRepository.AddAsync(newIngredient);
        return newIngredient;
    }
}