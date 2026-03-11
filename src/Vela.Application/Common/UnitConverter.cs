namespace Vela.Application.Common;

public static class UnitConverter
{
    // Konverterer alt til en "base-enhed" (gram eller ml) for at kunne lægge dem sammen
    public static (double BaseQuantity, string BaseUnit) Normalize(double quantity, string? unit)
    {
        if (string.IsNullOrEmpty(unit)) return (quantity, "stk");

        return unit.ToLower() switch
        {
            "kg" => (quantity * 1000, "g"),
            "l" => (quantity * 1000, "ml"),
            "dl" => (quantity * 100, "ml"),
            "cl" => (quantity * 10, "ml"),
            "spsk" => (quantity * 15, "ml"),
            "tsk" => (quantity * 5, "ml"),
            _ => (quantity, unit.ToLower()) // Returner som den er, hvis ukendt
        };
    }
}