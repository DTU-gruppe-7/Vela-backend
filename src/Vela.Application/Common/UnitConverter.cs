namespace Vela.Application.Common;

public static class UnitConverter
{
    public static (double BaseQuantity, string BaseUnit) Normalize(double? quantity, string? unit)
    {
        if (!quantity.HasValue || quantity == 0)
        {
            return (0, string.Empty);
        }

        double val = quantity.Value;
        
        if (string.IsNullOrWhiteSpace(unit)) 
            return (val, "stk");
        
        if (Conversions.TryGetValue(unit, out var conversion))
        {
            return (val * conversion.Factor, conversion.TargetUnit);
        }
        
        return (val, unit.ToLower());
    }
    
    private static readonly Dictionary<string, (double Factor, string TargetUnit)> Conversions = 
        new(StringComparer.OrdinalIgnoreCase) // Gør at den er ligeglad med store/små bogstaver
        {
            { "kg",   (1000, "g") },
            { "l",    (1000, "ml") },
            { "dl",   (100, "ml") },
            { "cl",   (10, "ml") },
            { "spsk", (15, "ml") },
            { "tsk",  (5, "ml") }
        };
}