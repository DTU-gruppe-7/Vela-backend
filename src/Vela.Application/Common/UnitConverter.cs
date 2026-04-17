using Vela.Domain.Enums;

namespace Vela.Application.Common;

public static class UnitConverter
{
    public static (double BaseQuantity, string BaseUnit) Normalize(double? quantity, string? unit, string? targetUnit, IngredientCategory? category = null)
    {
        if (!quantity.HasValue || quantity == 0)
        {
            return (0, string.Empty);
        }

        double val = quantity.Value;
        
        // If no unit provided, default to target unit or "stk"
        if (string.IsNullOrWhiteSpace(unit))
        {
            var defaultUnit = string.IsNullOrWhiteSpace(targetUnit) ? "stk" : targetUnit;
            return (val, defaultUnit);
        }
        
        // If no target unit specified or units match, return as-is (but spoon units become ml)
        if (string.IsNullOrWhiteSpace(targetUnit) ||
            unit.Equals(targetUnit, StringComparison.OrdinalIgnoreCase))
        {
            return SpoonToMlIfNeeded(val, unit.ToLower());
        }

        // Try to convert from source unit to target unit
        if (TryConvert(val, unit, targetUnit, category, out var convertedQuantity))
        {
            return (convertedQuantity, targetUnit.ToLower());
        }

        // If conversion not possible, return original values (but spoon units become ml)
        return SpoonToMlIfNeeded(val, unit.ToLower());
    }
    
    private static bool TryConvert(double quantity, string fromUnit, string toUnit, IngredientCategory? category, out double result)
    {
        result = 0;
        
        // Normalize unit names
        fromUnit = fromUnit.ToLower();
        toUnit = toUnit.ToLower();
        
        // Same unit, no conversion needed
        if (fromUnit == toUnit)
        {
            result = quantity;
            return true;
        }
        
        // Try to find conversion path within same unit family
        if (UnitFactors.TryGetValue(fromUnit, out var fromFactor) &&
            UnitFactors.TryGetValue(toUnit, out var toFactor))
        {
            // Check if units are in the same "family" (same base unit)
            if (fromFactor.BaseUnit == toFactor.BaseUnit)
            {
                // Convert: quantity * fromFactor.Factor / toFactor.Factor
                result = quantity * fromFactor.Factor / toFactor.Factor;
                return true;
            }

            // Convert spoon units to gram targets using the same app fallback as liquid conversions.
            if (IsSpoonUnit(fromUnit) && toFactor.BaseUnit == "g")
            {
                var milliliters = quantity * fromFactor.Factor;
                var grams = milliliters;
                result = grams / toFactor.Factor;
                return true;
            }
            
            // Handle cross-family conversions for liquids (weight ↔ volume)
            // Assumption: 1 kg = 1 liter (1000g = 1000ml) for water-based liquids
            if (ShouldUseLiquidConversion(category))
            {
                var fromIsWeight = fromFactor.BaseUnit == "g";
                var toIsVolume = toFactor.BaseUnit == "ml";
                var fromIsVolume = fromFactor.BaseUnit == "ml";
                var toIsWeight = toFactor.BaseUnit == "g";
                
                if (fromIsWeight && toIsVolume)
                {
                    // Convert weight to volume: kg/g → l/ml
                    // 1g = 1ml for liquids
                    var grams = quantity * fromFactor.Factor;
                    var milliliters = grams; // 1:1 ratio
                    result = milliliters / toFactor.Factor;
                    return true;
                }
                else if (fromIsVolume && toIsWeight)
                {
                    // Convert volume to weight: l/ml → kg/g
                    // 1ml = 1g for liquids
                    var milliliters = quantity * fromFactor.Factor;
                    var grams = milliliters; // 1:1 ratio
                    result = grams / toFactor.Factor;
                    return true;
                }
            }
        }
        
        return false;
    }

    private static (double Quantity, string Unit) SpoonToMlIfNeeded(double val, string unit) =>
        unit switch
        {
            "spsk" => (val * 15, "ml"),
            "tsk"  => (val * 5, "ml"),
            _      => (val, unit)
        };

    private static bool IsSpoonUnit(string unit) => unit is "spsk" or "tsk";
    
    private static bool ShouldUseLiquidConversion(IngredientCategory? category)
    {
        if (!category.HasValue)
            return false;
            
        // Categories that typically use liquid measurements
        return category.Value switch
        {
            IngredientCategory.Dairy => true,      // Milk, cream, yogurt, etc.
            IngredientCategory.Beverages => true,  // Juice, water, etc.
            IngredientCategory.OilsAndFats => true, // Cooking oil, etc.
            IngredientCategory.Condiments => true,  // Soy sauce, vinegar, etc.
            _ => false
        };
    }
    
    // Maps each unit to its conversion factor to base unit and the base unit itself
    private static readonly Dictionary<string, (string BaseUnit, double Factor)> UnitFactors = 
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Weight units (base: g)
            { "g",    ("g", 1) },
            { "kg",   ("g", 1000) },
            
            // Volume units (base: ml)
            { "ml",   ("ml", 1) },
            { "l",    ("ml", 1000) },
            { "dl",   ("ml", 100) },
            { "cl",   ("ml", 10) },
            { "spsk", ("ml", 15) },
            { "tsk",  ("ml", 5) },
            
            // Piece units (base: stk)
            { "stk",  ("stk", 1) }
        };
}