using FluentAssertions;
using Moq;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.Service;
using Vela.Application.Services;
using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Entities.ShoppingList;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Services;

public class ShoppingListServiceTests
{
	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenIngredientAppearsTwice_CombinesQuantitiesIntoSingleItem()
	{
		// Arrange
		var saltId = Guid.NewGuid();
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var ingredientA = new Ingredient
		{
			Id = saltId,
			Name = "Salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(ingredientA, 2, "g"),
				CreateRecipeIngredient(ingredientA, 3, "g")
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].IngredientName.Should().Be("Salt");
		context.AddedItems[0].Unit.Should().Be("g");
		context.AddedItems[0].Quantity.Should().BeApproximately(5, 0.0001);
		context.Entry.AddedToShoppingList.Should().BeTrue();
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenIngredientNamesOnlyDifferByCaseAndWhitespace_CombinesQuantities()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var ingredientA = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};
		var ingredientB = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = " salt ",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(ingredientA, 1, "g"),
				CreateRecipeIngredient(ingredientB, 4, "g")
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].Quantity.Should().BeApproximately(5, 0.0001);
		context.AddedItems[0].Unit.Should().Be("g");
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenIngredientNameMatchesAcrossCase_AlwaysUsesCanonicalDbUnit()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var saltA = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};
		var saltB = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = " salt ",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(saltA, 2, "g"),
				CreateRecipeIngredient(saltB, 3, "g")
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].Unit.Should().Be("g");
		context.AddedItems[0].Quantity.Should().BeApproximately(5, 0.0001);
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenOneIngredientLineHasNoUnit_UsesDbUnitAndCombines()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var salt = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(salt, 2, "g"),
				CreateRecipeIngredient(salt, 3, null)
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].Unit.Should().Be("g");
		context.AddedItems[0].Quantity.Should().BeApproximately(5, 0.0001);
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenIngredientUsesSpskAndTsk_ConvertsToDbUnitAndCombines()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var salt = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(salt, 1, "spsk"),
				CreateRecipeIngredient(salt, 1, "tsk")
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].Unit.Should().Be("g");
		context.AddedItems[0].Quantity.Should().BeApproximately(20, 0.0001);
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenDbUnitIsStk_ConvertsSpoonUnitToMl()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var flour = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Mel",
			Unit = "stk",
			Category = IngredientCategory.Grains
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(flour, 2, "spsk")
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert: spsk must be converted to ml (2 spsk = 30 ml), never kept as spsk/tsk
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].Unit.Should().Be("ml");
		context.AddedItems[0].Quantity.Should().BeApproximately(30, 0.0001);
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenSameIngredientHasMlAndG_UsesGAsWinningUnit()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var spiceAsMl = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Salt",
			Unit = "ml",
			Category = IngredientCategory.HerbsAndSpices
		};
		var spiceAsG = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(spiceAsMl, 10, "ml"),
				CreateRecipeIngredient(spiceAsG, 5, "g")
			});

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].Unit.Should().Be("g");
		context.AddedItems[0].Quantity.Should().BeApproximately(15, 0.0001);
	}

	[Fact]
	public async Task GenerateFromMealPlanAsync_WhenIngredientNameIsExcluded_SkipsIngredient()
	{
		// Arrange
		var mealPlanId = Guid.NewGuid();
		var callerUserId = "user-1";
		var water = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Vand",
			Unit = "ml",
			Category = IngredientCategory.Beverages
		};
		var salt = new Ingredient
		{
			Id = Guid.NewGuid(),
			Name = "Salt",
			Unit = "g",
			Category = IngredientCategory.HerbsAndSpices
		};

		var context = BuildGenerateContext(
			mealPlanId,
			callerUserId,
			new List<RecipeIngredient>
			{
				CreateRecipeIngredient(water, 200, "ml"),
				CreateRecipeIngredient(salt, 5, "g")
			},
			excludedIngredientNames: new[] { "vand" });

		// Act
		var result = await context.Sut.GenerateFromMealPlanAsync(
			mealPlanId,
			new DateOnly(2026, 4, 1),
			new DateOnly(2026, 4, 30),
			null,
			callerUserId);

		// Assert
		result.Success.Should().BeTrue();
		context.AddedItems.Should().HaveCount(1);
		context.AddedItems[0].IngredientName.Should().Be("Salt");
	}

	private static GenerateContext BuildGenerateContext(
		Guid mealPlanId,
		string userId,
		List<RecipeIngredient> recipeIngredients,
		IEnumerable<string>? excludedIngredientNames = null)
	{
		var shoppingListRepositoryMock = new Mock<IShoppingListRepository>();
		var ingredientRepositoryMock = new Mock<IIngredientRepository>();
		var mealPlanRepositoryMock = new Mock<IMealPlanRepository>();
		var groupRepositoryMock = new Mock<IGroupRepository>();
		var groupAuthorizationServiceMock = new Mock<IGroupAuthorizationService>();
		var ingredientExclusionProviderMock = new Mock<IShoppingListIngredientExclusionProvider>();

		var excludedNames = (excludedIngredientNames ?? Array.Empty<string>())
			.Select(x => x.Trim().ToLowerInvariant())
			.ToHashSet(StringComparer.OrdinalIgnoreCase);
		ingredientExclusionProviderMock
			.Setup(x => x.IsExcluded(It.IsAny<string>()))
			.Returns<string>(name => excludedNames.Contains(name.Trim().ToLowerInvariant()));

		var recipe = new Recipe
		{
			Id = Guid.NewGuid(),
			Name = "Test Recipe",
			ServingSize = 1,
			Ingredients = recipeIngredients
		};

		foreach (var ingredient in recipeIngredients)
		{
			ingredient.Recipe = recipe;
			ingredient.RecipeId = recipe.Id;
		}

		var mealPlan = new MealPlan
		{
			Id = mealPlanId,
			UserId = userId,
			Entries = new List<MealPlanEntry>()
		};

		var entry = new MealPlanEntry
		{
			Id = Guid.NewGuid(),
			MealPlanId = mealPlan.Id,
			MealPlan = mealPlan,
			RecipeId = recipe.Id,
			Recipe = recipe,
			Date = new DateOnly(2026, 4, 10),
			MealType = "Dinner",
			Servings = 1
		};

		mealPlan.Entries.Add(entry);

		var shoppingList = new ShoppingList
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Name = "Test List",
			Items = new List<ShoppingListItem>()
		};

		var addedItems = new List<ShoppingListItem>();

		mealPlanRepositoryMock
			.Setup(x => x.GetByIdWithEntriesByDateRangeAsync(mealPlanId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
			.ReturnsAsync(mealPlan);

		shoppingListRepositoryMock
			.Setup(x => x.GetByUserIdAsync(userId))
			.ReturnsAsync(shoppingList);

		shoppingListRepositoryMock
			.Setup(x => x.AddItemAsync(It.IsAny<ShoppingListItem>()))
			.Callback<ShoppingListItem>(item =>
			{
				addedItems.Add(item);
				shoppingList.Items!.Add(item);
			})
			.ReturnsAsync((ShoppingListItem item) => item);

		shoppingListRepositoryMock
			.Setup(x => x.GetByIdWithItemsAsync(shoppingList.Id))
			.ReturnsAsync(shoppingList);

		shoppingListRepositoryMock
			.Setup(x => x.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var sut = new ShoppingListService(
			shoppingListRepositoryMock.Object,
			ingredientRepositoryMock.Object,
			mealPlanRepositoryMock.Object,
			groupRepositoryMock.Object,
			groupAuthorizationServiceMock.Object,
			ingredientExclusionProviderMock.Object);

		return new GenerateContext(sut, entry, addedItems);
	}

	private static RecipeIngredient CreateRecipeIngredient(Ingredient ingredient, double quantity, string? unit) => new()
	{
		Id = Guid.NewGuid(),
		IngredientId = ingredient.Id,
		Ingredient = ingredient,
		Quantity = quantity,
		Unit = unit,
		RawMeasure = string.IsNullOrWhiteSpace(unit) ? $"{quantity}" : $"{quantity} {unit}",
		RecipeId = Guid.Empty,
		Recipe = new Recipe { Id = Guid.NewGuid(), Name = "Placeholder" }
	};

	private sealed record GenerateContext(
		ShoppingListService Sut,
		MealPlanEntry Entry,
		List<ShoppingListItem> AddedItems);
}

