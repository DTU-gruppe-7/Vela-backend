using FluentAssertions;
using Vela.Application.Validation;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Validation;

public class IngredientValidatorTests
{
    private readonly IngredientValidator _sut = new();

    [Fact]
    public void ValidateName_WhenEmpty_ReturnsError()
    {
        var result = _sut.ValidateName(" ");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void ValidateName_WhenTooLong_ReturnsError()
    {
        var result = _sut.ValidateName(new string('a', 201));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("exceeds maximum length"));
    }

    [Fact]
    public void ValidateName_WhenWhitespaceAtEnds_ReturnsWarning()
    {
        var result = _sut.ValidateName(" Salt ");

        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("leading or trailing"));
    }

    [Fact]
    public void ValidateName_WhenContainsMeasurements_ReturnsWarning()
    {
        var result = _sut.ValidateName("100 gram mel");

        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("contains measurement units"));
    }

    [Fact]
    public void ValidateName_WhenContainsControlCharacters_ReturnsError()
    {
        var result = _sut.ValidateName("Salt\x0B"); // \x0B is vertical tab, a control char

        result.Errors.Should().Contain(e => e.Contains("invalid control characters"));
    }

    [Fact]
    public void ValidateName_WhenValid_ReturnsNoErrorsOrWarnings()
    {
        var result = _sut.ValidateName("Garam Masala");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ValidateUnit_WhenEmpty_ReturnsError()
    {
        var result = _sut.ValidateUnit("", IngredientCategory.HerbsAndSpices);

        result.IsValid.Should().BeFalse(); // Because string.IsNullOrWhiteSpace returns result directly
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void ValidateUnit_WhenInvalidUnit_ReturnsError()
    {
        var result = _sut.ValidateUnit("bunch", IngredientCategory.HerbsAndSpices);

        result.Errors.Should().Contain(e => e.Contains("Invalid unit"));
    }

    [Fact]
    public void ValidateUnit_WhenBeverageWithG_ReturnsWarning()
    {
        var result = _sut.ValidateUnit("g", IngredientCategory.Beverages);

        result.Warnings.Should().Contain(w => w.Contains("typically use 'ml'"));
    }

    [Fact]
    public void ValidateConsistency_WhenDairyAndVegan_ReturnsError()
    {
        var ingredient = new Ingredient { Name = "Mælk", Category = IngredientCategory.Dairy, IsVegan = true };
        
        var result = _sut.ValidateConsistency(ingredient);

        result.Errors.Should().Contain(e => e.Contains("logical contradiction"));
    }

    [Fact]
    public void ValidateConsistency_WhenMeatAndVegan_ReturnsError()
    {
        var ingredient = new Ingredient { Name = "Kylling", Category = IngredientCategory.Meat, IsVegan = true };
        
        var result = _sut.ValidateConsistency(ingredient);

        result.Errors.Should().Contain(e => e.Contains("logical contradiction"));
    }

    [Fact]
    public void ValidateConsistency_WhenCategoryOther_ReturnsWarning()
    {
        var ingredient = new Ingredient { Name = "Ting", Category = IngredientCategory.Other };
        
        var result = _sut.ValidateConsistency(ingredient);

        result.Warnings.Should().Contain(w => w.Contains("categorized as 'Other'"));
    }
}
