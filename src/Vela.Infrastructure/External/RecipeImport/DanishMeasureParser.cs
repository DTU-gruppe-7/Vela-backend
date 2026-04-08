using System.Globalization;
using System.Text.RegularExpressions;

namespace Vela.Infrastructure.External.RecipeImport;

public static class DanishMeasureParser
{
	// Maps all known unit forms (including plurals and variants) → canonical unit string
	private static readonly Dictionary<string, string> UnitAliases = new(StringComparer.OrdinalIgnoreCase)
	{
		// Canonical forms
		{ "g", "g" }, { "kg", "kg" }, { "dl", "dl" }, { "cl", "cl" }, { "ml", "ml" }, { "l", "l" },
		{ "tsk", "tsk" }, { "spsk", "spsk" }, { "stk", "stk" }, { "fed", "fed" },
		{ "knivspids", "knivspids" }, { "nip", "nip" }, { "bundt", "bundt" },
		{ "dåse", "dåse" }, { "pakke", "pakke" }, { "pose", "pose" },
		{ "blad", "blad" }, { "stilk", "stilk" }, { "skive", "skive" }, { "håndfuld", "håndfuld" },

		// Aliases and plural forms
		{ "liter", "l" },
		{ "stængler", "stilk" }, { "stangel", "stilk" }, { "stængel", "stilk" },
		{ "håndfulde", "håndfuld" },
		{ "dråbe", "dråbe" }, { "dråber", "dråbe" },
		{ "drys", "drys" },
		{ "dryp", "dryp" },
		{ "bakke", "bakke" },
		{ "dåser", "dåse" },
		{ "pakker", "pakke" },
		{ "ark", "stk" },
		{ "glas", "glas" },
		{ "skefuld", "spsk" }, { "skefulde", "spsk" },
		{ "portion", "portion" },
		{ "kvist", "kvist" }, { "kviste", "kvist" },
	};

	// Ingredient names that start with these prefixes are junk (instructions, cross-references, equipment)
	private static readonly string[] JunkPrefixes =
	[
		"til ", "evt", "af ", "+ ", "tilbehør", "lunkent vand", "kogende vand",
		"stuetempereret vand", "stuetemperet vand",
	];

	// Ingredient names containing these substrings are junk
	private static readonly string[] JunkSubstrings =
	[
		" fra ", "til at ", "- se ", "kan undlades", "til pensling", "til servering",
		"til pynt", "til at smage", "til at røre", "til at fylde", "til konsistens",
		"vand til", "- ca ", "handskerne", " + ", "kogende", "en gryde",
		"en masse", "så meget du har", "efter eget valg",
	];

	// Known equipment and non-food items
	private static readonly HashSet<string> JunkExact = new(StringComparer.OrdinalIgnoreCase)
	{
		"træspyd", "grillspyd", "konfektforme", "topping",
		"bomuldssnor", "engangshandsker", "kageplast", "guldglimmer",
		"derudover",
	};

	// Combinations with "og" that are non-ingredient compounds
	private static readonly HashSet<string> JunkCompounds = new(StringComparer.OrdinalIgnoreCase)
	{
		"isterninger og vand", "vand og isterninger", "vand + evt en isterning",
		"vand + evt 1 isterning", "brød eller boller",
	};

	/// <summary>
	/// Returns true if the ingredient name is junk (an instruction, cross-reference, or equipment)
	/// and should be skipped during import.
	/// </summary>
	public static bool IsJunk(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return true;

		var lower = name.ToLowerInvariant().Trim();

		if (JunkExact.Contains(lower))
			return true;

		if (JunkCompounds.Contains(lower))
			return true;

		foreach (var prefix in JunkPrefixes)
			if (lower.StartsWith(prefix, StringComparison.Ordinal))
				return true;

		foreach (var sub in JunkSubstrings)
			if (lower.Contains(sub, StringComparison.Ordinal))
				return true;

		return false;
	}

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
			// Ingen mængde fundet – hele strengen er ingrediensnavnet (strip optional annotation after comma)
			return (0, "", raw.Split(',')[0].Trim());
		}

		string numberStr = match.Groups[1].Value.Replace(',', '.');
		double quantity = double.TryParse(numberStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var q) ? q : 0;
		string rest = match.Groups[2].Value.Trim();

		// Prøv at finde en enhed i starten af "rest"
		var unitMatch = Regex.Match(rest, @"^(\S+)\s+(.+)$");
		if (unitMatch.Success)
		{
			string possibleUnit = unitMatch.Groups[1].Value;
			if (UnitAliases.TryGetValue(possibleUnit, out var canonicalUnit))
			{
				string ingredientName = unitMatch.Groups[2].Value.Trim();
				// Fjern eventuelle ekstra beskrivelser efter komma (f.eks. "smør, til at stege i" → "smør")
				string cleanName = ingredientName.Split(',')[0].Trim();
				return (quantity, canonicalUnit, cleanName);
			}
		}

		// Ingen kendt enhed – "rest" er ingrediensnavnet (f.eks. "2 æg" → unit="stk")
		string name = rest.Split(',')[0].Trim();
		return (quantity, "stk", name);
	}
}
