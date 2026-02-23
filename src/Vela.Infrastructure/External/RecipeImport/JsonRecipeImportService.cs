using System.Text.Json;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.External;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities;

namespace Vela.Infrastructure.External.RecipeImport;

public class JsonRecipeImportService : IRecipeImportService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IIngredientRepository _ingredientRepository;

    public JsonRecipeImportService(
        IRecipeRepository recipeRepository,
        IIngredientRepository ingredientRepository)
    {
        _recipeRepository = recipeRepository;
        _ingredientRepository = ingredientRepository;
    }

    public async Task ImportRecipesFromJsonAsync()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData", "recipes.json");

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Kan ikke finde recipe-filen: {jsonPath}");

        var json = await File.ReadAllTextAsync(jsonPath);
        var recipeDtos = JsonSerializer.Deserialize<List<JsonRecipeDto>>(json)
                         ?? throw new InvalidOperationException("Kunne ikke deserialisere recipes.json");

        foreach (var dto in recipeDtos)
        {
            // Tjek om opskriften allerede er importeret (baseret på navn)
            if (await _recipeRepository.ExistsByNameAsync(dto.Titel))
                continue;

            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Name = dto.Titel,
                Description = dto.Beskrivelse,
                Category = dto.Kategori,
                ThumbnailUrl = dto.Billede,
                SourceUrl = dto.KildeUrl,
                ServingSize = int.TryParse(dto.AntalPersoner, out var s) ? s : 0,
                TotalTime = dto.TidIAlt,
                WorkTime = dto.Arbejdstid,
                InstructionsJson = JsonSerializer.Serialize(dto.Instruktioner),
                KeywordsJson = JsonSerializer.Serialize(dto.Noegleord),
                Ingredients = new List<RecipeIngredient>()
            };

            await ProcessIngredientsAsync(recipe, dto.Ingredienser);
            await _recipeRepository.AddAsync(recipe);
        }
    }

    private async Task ProcessIngredientsAsync(Recipe recipe, Dictionary<string, List<string>> sections)
    {
        foreach (var (section, lines) in sections)
        {
            foreach (var line in lines)
            {
                var (quantity, unit, ingredientName) = DanishMeasureParser.Parse(line);

                if (string.IsNullOrWhiteSpace(ingredientName))
                    continue;

                var ingredient = await GetOrCreateIngredientAsync(ingredientName);

                var recipeIngredient = new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    RecipeId = recipe.Id,
                    Recipe = recipe,
                    IngredientId = ingredient.Id,
                    Ingredient = ingredient,
                    Quantity = quantity,
                    Unit = unit,
                    Section = section,
                    RawMeasure = line
                };

                recipe.Ingredients.Add(recipeIngredient);
            }
        }
    }

    private async Task<Ingredient> GetOrCreateIngredientAsync(string name)
    {
        var normalizedName = name.ToLower().Trim();
        var existing = await _ingredientRepository.GetByNameAsync(normalizedName);

        if (existing != null)
            return existing;

        var newIngredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            ContainsGluten = false,
            ContainsLactose = false,
            ContainsNuts = false,
            IsVegan = false
        };

        await _ingredientRepository.AddAsync(newIngredient);
        return newIngredient;
    }
}