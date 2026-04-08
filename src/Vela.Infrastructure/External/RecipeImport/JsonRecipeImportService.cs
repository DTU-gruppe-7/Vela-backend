using System.Text.Json;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.External;
using Vela.Application.Interfaces.Repository;
using Vela.Domain.Entities.Recipe;

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

        // Lokalt dictionary til at cache ingredienser – undgår dubletter og unødvendige DB-opslag
        var ingredientCache = new Dictionary<string, Ingredient>();

        foreach (var dto in recipeDtos)
        {
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

            await ProcessIngredientsAsync(recipe, dto.Ingredienser, ingredientCache);
            await _recipeRepository.AddAsync(recipe);
        }

        await _recipeRepository.SaveChangesAsync();
    }

    private async Task ProcessIngredientsAsync(
        Recipe recipe,
        Dictionary<string, List<string>> sections,
        Dictionary<string, Ingredient> ingredientCache)
    {
        foreach (var (section, lines) in sections)
        {
            foreach (var line in lines)
            {
                var (quantity, unit, ingredientName) = DanishMeasureParser.Parse(line);

                if (string.IsNullOrWhiteSpace(ingredientName) || DanishMeasureParser.IsJunk(ingredientName))
                    continue;

                var ingredient = await GetOrCreateIngredientAsync(ingredientName, unit, ingredientCache);

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

    private async Task<Ingredient> GetOrCreateIngredientAsync(
        string name,
        string parsedUnit,
        Dictionary<string, Ingredient> ingredientCache)
    {
        var normalizedName = name.ToLower().Trim();

        // 1. Tjek lokalt cache først
        if (ingredientCache.TryGetValue(normalizedName, out var cached))
            return cached;

        // 2. Tjek databasen
        var existing = await _ingredientRepository.GetByNameAsync(normalizedName);
        if (existing != null)
        {
            ingredientCache[normalizedName] = existing;
            return existing;
        }

        // 3. Klassificer og opret ny ingredient
        var category = IngredientClassifier.Classify(normalizedName);
        var (containsGluten, containsLactose, containsNuts, isVegan) = IngredientClassifier.DetectAllergens(normalizedName);
        var defaultUnit = IngredientClassifier.GetDefaultUnit(normalizedName, category);

        var newIngredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Unit = defaultUnit,
            Category = category,
            ContainsGluten = containsGluten,
            ContainsLactose = containsLactose,
            ContainsNuts = containsNuts,
            IsVegan = isVegan,
        };

        await _ingredientRepository.AddAsync(newIngredient);
        ingredientCache[normalizedName] = newIngredient;
        return newIngredient;
    }
}