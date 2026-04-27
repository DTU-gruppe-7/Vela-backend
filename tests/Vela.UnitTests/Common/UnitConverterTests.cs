using FluentAssertions;
using Vela.Application.Common;
using Vela.Domain.Enums;

namespace Vela.UnitTests.Common;

public class UnitConverterTests
{
    // ───────────────────── Null / Zero ─────────────────────

    [Fact]
    public void Normalize_WhenQuantityIsNull_ReturnsZero()
    {
        var (quantity, unit) = UnitConverter.Normalize(null, "g", "g");

        quantity.Should().Be(0);
        unit.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_WhenQuantityIsZero_ReturnsZero()
    {
        var (quantity, unit) = UnitConverter.Normalize(0, "g", "g");

        quantity.Should().Be(0);
        unit.Should().BeEmpty();
    }

    // ───────────────────── No unit supplied ─────────────────────

    [Fact]
    public void Normalize_WhenNoUnitAndNoTarget_DefaultsToStk()
    {
        var (quantity, unit) = UnitConverter.Normalize(5, null, null);

        quantity.Should().Be(5);
        unit.Should().Be("stk");
    }

    [Fact]
    public void Normalize_WhenNoUnitButHasTarget_UsesTargetUnit()
    {
        var (quantity, unit) = UnitConverter.Normalize(5, null, "g");

        quantity.Should().Be(5);
        unit.Should().Be("g");
    }

    [Fact]
    public void Normalize_WhenUnitIsWhitespace_DefaultsToTarget()
    {
        var (quantity, unit) = UnitConverter.Normalize(5, "  ", "ml");

        quantity.Should().Be(5);
        unit.Should().Be("ml");
    }

    // ───────────────────── Same unit / no target ─────────────────────

    [Fact]
    public void Normalize_WhenSourceEqualsTarget_ReturnsUnchanged()
    {
        var (quantity, unit) = UnitConverter.Normalize(100, "g", "g");

        quantity.Should().Be(100);
        unit.Should().Be("g");
    }

    [Fact]
    public void Normalize_WhenSourceEqualsTargetCaseInsensitive_ReturnsUnchanged()
    {
        var (quantity, unit) = UnitConverter.Normalize(100, "G", "g");

        quantity.Should().Be(100);
        unit.Should().Be("g");
    }

    // ───────────────────── Weight conversions ─────────────────────

    [Fact]
    public void Normalize_WhenKgToG_MultipliesBy1000()
    {
        var (quantity, unit) = UnitConverter.Normalize(2, "kg", "g");

        quantity.Should().BeApproximately(2000, 0.0001);
        unit.Should().Be("g");
    }

    [Fact]
    public void Normalize_WhenGToKg_DividesBy1000()
    {
        var (quantity, unit) = UnitConverter.Normalize(500, "g", "kg");

        quantity.Should().BeApproximately(0.5, 0.0001);
        unit.Should().Be("kg");
    }

    // ───────────────────── Volume conversions ─────────────────────

    [Fact]
    public void Normalize_WhenDlToMl_MultipliesBy100()
    {
        var (quantity, unit) = UnitConverter.Normalize(2, "dl", "ml");

        quantity.Should().BeApproximately(200, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenLToMl_MultipliesBy1000()
    {
        var (quantity, unit) = UnitConverter.Normalize(1.5, "l", "ml");

        quantity.Should().BeApproximately(1500, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenClToMl_MultipliesBy10()
    {
        var (quantity, unit) = UnitConverter.Normalize(5, "cl", "ml");

        quantity.Should().BeApproximately(50, 0.0001);
        unit.Should().Be("ml");
    }

    // ───────────────────── Spoon unit conversions ─────────────────────

    [Fact]
    public void Normalize_WhenSpskNoTarget_ConvertsToMl()
    {
        var (quantity, unit) = UnitConverter.Normalize(2, "spsk", null);

        quantity.Should().BeApproximately(30, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenTskNoTarget_ConvertsToMl()
    {
        var (quantity, unit) = UnitConverter.Normalize(3, "tsk", null);

        quantity.Should().BeApproximately(15, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenSpskToMl_ConvertsCorrectly()
    {
        var (quantity, unit) = UnitConverter.Normalize(2, "spsk", "ml");

        quantity.Should().BeApproximately(30, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenTskToMl_ConvertsCorrectly()
    {
        var (quantity, unit) = UnitConverter.Normalize(2, "tsk", "ml");

        quantity.Should().BeApproximately(10, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenSpskToG_ConvertsViaMilliliters()
    {
        // 1 spsk = 15 ml, and spoon→g uses 1:1 ratio
        var (quantity, unit) = UnitConverter.Normalize(1, "spsk", "g");

        quantity.Should().BeApproximately(15, 0.0001);
        unit.Should().Be("g");
    }

    [Fact]
    public void Normalize_WhenTskToG_ConvertsViaMilliliters()
    {
        // 1 tsk = 5 ml, and spoon→g uses 1:1 ratio
        var (quantity, unit) = UnitConverter.Normalize(1, "tsk", "g");

        quantity.Should().BeApproximately(5, 0.0001);
        unit.Should().Be("g");
    }

    // ───────────────────── Liquid conversion (cross-family) ─────────────────────

    [Fact]
    public void Normalize_WhenGToMlForDairy_UsesLiquidConversion()
    {
        var (quantity, unit) = UnitConverter.Normalize(200, "g", "ml", IngredientCategory.Dairy);

        quantity.Should().BeApproximately(200, 0.0001);
        unit.Should().Be("ml");
    }

    [Fact]
    public void Normalize_WhenMlToGForBeverages_UsesLiquidConversion()
    {
        var (quantity, unit) = UnitConverter.Normalize(100, "ml", "g", IngredientCategory.Beverages);

        quantity.Should().BeApproximately(100, 0.0001);
        unit.Should().Be("g");
    }

    [Fact]
    public void Normalize_WhenGToMlForMeat_DoesNotUseLiquidConversion()
    {
        // Meat is not a liquid category, so g→ml conversion should fail
        var (quantity, unit) = UnitConverter.Normalize(200, "g", "ml", IngredientCategory.Meat);

        // Should NOT convert — returns original value with spoon-to-ml fallback (or stays as-is)
        unit.Should().Be("g");
    }

    // ───────────────────── Unknown / non-convertible units ─────────────────────

    [Fact]
    public void Normalize_WhenStkToG_ReturnsOriginalUnit()
    {
        // stk and g are different families, no conversion path
        var (quantity, unit) = UnitConverter.Normalize(3, "stk", "g");

        quantity.Should().Be(3);
        unit.Should().Be("stk");
    }

    [Fact]
    public void Normalize_WhenUnknownUnit_ReturnsAsIs()
    {
        var (quantity, unit) = UnitConverter.Normalize(1, "bunch", "g");

        quantity.Should().Be(1);
        unit.Should().Be("bunch");
    }
}
