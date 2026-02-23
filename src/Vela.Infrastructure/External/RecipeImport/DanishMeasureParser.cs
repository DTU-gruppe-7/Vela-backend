using System.Globalization;
using System.Text.RegularExpressions;

namespace Vela.Infrastructure.External.RecipeImport;

public static class DanishMeasureParser
{
    // Kendte enheder i danske opskrifter
    private static readonly HashSet<string> KnownUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        "g", "kg", "dl", "cl", "ml", "l", "tsk", "spsk", "stk", "skiver",
        "fed", "knivspids", "bundt", "dåse", "pakke", "pose", "blad",
        "stilk", "nip", "håndfuld", "skive"
    };

    /// <summary>
    /// Parser en dansk ingredienslinje som "150 g frosne blåbær" 
    /// og returnerer (quantity, unit, ingredientName).
    /// </summary>
    public static (double Quantity, string Unit, string IngredientName) Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (0, "", raw ?? "");

        raw = raw.Trim();

        // Match et tal i starten (dansk format med komma, f.eks. "0,50")
        var match = Regex.Match(raw, @"^(\d+(?:[,\.]\d+)?)\s+(.+)$");

        if (!match.Success)
        {
            // Ingen mængde fundet – hele strengen er ingrediensnavnet
            return (0, "", raw);
        }

        string numberStr = match.Groups[1].Value.Replace(',', '.');
        double quantity = double.TryParse(numberStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var q) ? q : 0;
        string rest = match.Groups[2].Value.Trim();

        // Prøv at finde en enhed i starten af "rest"
        var unitMatch = Regex.Match(rest, @"^(\S+)\s+(.+)$");
        if (unitMatch.Success)
        {
            string possibleUnit = unitMatch.Groups[1].Value;
            if (KnownUnits.Contains(possibleUnit))
            {
                string ingredientName = unitMatch.Groups[2].Value.Trim();
                // Fjern eventuelle ekstra beskrivelser efter komma (f.eks. "smør, til at stege i" → "smør")
                string cleanName = ingredientName.Split(',')[0].Trim();
                return (quantity, possibleUnit, cleanName);
            }
        }

        // Ingen kendt enhed – "rest" er ingrediensnavnet (f.eks. "2 æg" → unit="stk")
        string name = rest.Split(',')[0].Trim();
        return (quantity, "stk", name);
    }
}