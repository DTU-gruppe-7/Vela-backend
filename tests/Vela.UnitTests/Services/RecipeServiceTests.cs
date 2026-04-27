using FluentAssertions;
using Moq;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Services;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepo = new();
    private readonly RecipeService _sut;

    public RecipeServiceTests()
    {
        _sut = new RecipeService(_recipeRepo.Object);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_WhenRecipeFound_ReturnsMappedDto()
    {
        var recipeId = Guid.NewGuid();
        var recipe = new Recipe
        {
            Id = recipeId,
            Name = "Test Recipe",
            Category = "Aftensmad",
            Ingredients = new List<RecipeIngredient>
            {
                new() { IngredientId = Guid.NewGuid(), Ingredient = new Ingredient { Name = "Salt" }, Quantity = 5, Unit = "g", Recipe = new Recipe { Id = recipeId, Name = "Test Recipe" }, RawMeasure = "5 g" }
            }
        };
        _recipeRepo.Setup(x => x.GetByIdWithIngredientsAsync(recipeId)).ReturnsAsync(recipe);

        var result = await _sut.GetRecipeByIdAsync(recipeId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(recipeId);
        result.Name.Should().Be("Test Recipe");
        result.Ingredients.Should().HaveCount(1);
        result.Ingredients[0].IngredientName.Should().Be("Salt");
    }

    [Fact]
    public async Task GetRecipeByIdAsync_WhenRecipeNotFound_ReturnsNull()
    {
        _recipeRepo.Setup(x => x.GetByIdWithIngredientsAsync(It.IsAny<Guid>())).ReturnsAsync((Recipe?)null);

        var result = await _sut.GetRecipeByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllRecipesAsync_WithParameters_PassesParametersToRepoAndMaps()
    {
        var recipes = new List<Recipe> { new() { Id = Guid.NewGuid(), Name = "A" } };
        var allergens = new List<RecipeAllergen> { RecipeAllergen.Nuts };
        _recipeRepo.Setup(x => x.GetAllSummariesAsync(allergens, true)).ReturnsAsync(recipes);

        var result = await _sut.GetAllRecipesAsync(allergens, true);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("A");
        _recipeRepo.Verify(x => x.GetAllSummariesAsync(allergens, true), Times.Once);
    }

    [Fact]
    public async Task GetNextRecipesAsync_WithParameters_PassesParametersToRepoAndMaps()
    {
        var recipes = new List<Recipe> { new() { Id = Guid.NewGuid(), Name = "B" } };
        var allergens = new List<RecipeAllergen> { RecipeAllergen.Gluten };
        _recipeRepo.Setup(x => x.GetNextRecipesAsync("user", 10, "Aftensmad", allergens, false)).ReturnsAsync(recipes);

        var result = await _sut.GetNextRecipesAsync("user", 10, "Aftensmad", allergens, false);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("B");
        _recipeRepo.Verify(x => x.GetNextRecipesAsync("user", 10, "Aftensmad", allergens, false), Times.Once);
    }

    [Fact]
    public async Task GetMostLikedRecipesAsync_ReturnsMappedDtos()
    {
        var recipes = new List<Recipe> { new() { Id = Guid.NewGuid(), Name = "C" } };
        _recipeRepo.Setup(x => x.GetMostLikedRecipesAsync(5, null, false)).ReturnsAsync(recipes);

        var result = await _sut.GetMostLikedRecipesAsync(5);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("C");
    }
}
