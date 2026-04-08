using Vela.Domain.Enums;

namespace Vela.Infrastructure.External.RecipeImport;

public static class IngredientClassifier
{
	// Checked before CategoryRules — prevents short-keyword false-positives inside longer words.
	// e.g. "ande" would match "koriander"/"mandel", "te" would match "teriyaki"/"kotelet", etc.
	private static readonly (string Substring, IngredientCategory Category)[] PriorityOverrides =
	[
		("koriander",    IngredientCategory.HerbsAndSpices),  // "ande" in "koriander"
		("korianderfrø", IngredientCategory.HerbsAndSpices),
		("mandel",       IngredientCategory.NutsAndSeeds),    // "ande" in "mandel*"
		("teriyaki",     IngredientCategory.Condiments),      // "te" in "teriyaki"
		("entrecote",    IngredientCategory.Meat),            // "te" in "entrecote"
		("kotelet",      IngredientCategory.Meat),            // "te" in "kotelet/koteletter"
		("nakkefilet",   IngredientCategory.Meat),
		("ølandshvede",  IngredientCategory.Grains),          // "øl" in "ølandshvede"
		("spareribs",    IngredientCategory.Meat),            // "ribs" in Fruits would fire first
	];

	// Priority-ordered: first match wins. Produce (Vegetables/Fruits) checked before Fish/Meat/Grains/Beverages
	// to prevent short keyword false-positives (ål→kål, mel→melon, vin→oliven/svine, and→vandmelon).
	private static readonly (IngredientCategory Category, string[] Keywords)[] CategoryRules =
	[
		(IngredientCategory.Eggs, [
			"æg", "æggeblomme", "æggehvide",
		]),
		(IngredientCategory.Vegetables, [
			// Kål family first — prevents "ål" (Fish) matching kål words
			"blomkål", "rødkål", "hvidkål", "grønkål", "spidskål", "rosenkål", "savoykål",
			"romanesco", "bruxelles", "kål",
			// Other vegetables
			"gulerod", "gulerødder", "løg", "hvidløg", "tomat", "agurk", "salat", "spinat",
			"broccoli", "kartofler", "kartoffel", "babykartofler", "porre", "selleri", "bladselleri",
			"peberfrugt", "squash", "aubergine", "svamp", "rødbede", "asparges", "artiskok",
			"majs", "chili", "pastinak", "zucchini", "fennikel", "radicchio", "sukkerærter",
			"radise", "endivie", "rucola", "pak choi", "bok choy", "romainesalat", "hjertesalat",
			"salatløg", "rødløg", "skalotteløg", "forårsløg", "purløg", "ærter", "bønner",
			"majskorn", "bolchebeder", "blommetomater", "baby spinat", "babyspinat", "tangsalat",
			"persillerod", "pastinak", "bønnespirer", "haricots verts", "kantareller",
			"shiitake", "karl johan", "røget peberrod", "peberrod",
		]),
		(IngredientCategory.Fruits, [
			// Melon family first — prevents "mel" (Grains) matching melon words
			"vandmelon", "cantaloupe", "galia melon", "melon",
			// Other fruits
			"æble", "pære", "banan", "appelsin", "blodappelsin", "citron", "lime",
			"jordbær", "hindbær", "blåbær", "kiwi", "mango", "ananas",
			"drue", "vindruer", "fersken", "abrikos", "blomme", "kirsebær",
			"granatæble", "kokosnød", "avocado", "passionsfrugt", "nektarin", "papaya",
			"guava", "solbær", "rød ribs", "hvide ribs", "stikkelsbær", "brombær", "hyben",
			"dadel", "svesker", "figner", "rosiner", "æblemos", "æblemost",
		]),
		(IngredientCategory.Fish, [
			"fisk", "laks", "tun", "rejer", "torsk", "rødspætte", "hellefisk", "sild",
			"makrel", "skaldyr", "muslinger", "blæksprutte", "kaviar", "krabbe", "hummer", "østers",
			"kuller", "dorade", "lakseside", "sildefilet", "torskefilet", "laksefilet", "rødspættefilet",
			"fiskefrikadelle", "fiskeben", "rogn", "jomfruhummer", "hummerhaler", "krabbeklø",
			"vannamei", "tigerrejer", "ansjos", "hellefisk", "ålefilet", "røget ål",
		]),
		(IngredientCategory.Meat, [
			"kød", "bøf", "svinekød", "okse", "kylling", "kalkun", "bacon", "pølse", "lam",
			"hakkekød", "schnitzel", "skinke", "chorizo", "salami", "prosciutto",
			"pancetta", "mortadella", "svinemørbrad", "oksemørbrad", "oksehale", "okseskank",
			"oksekæbe", "okseinderlår", "ribbensteg", "nakkekoteletter", "svinekæbe", "svineslag",
			"svineben", "lammekoteletter", "lammekrone", "lammeculotte",
			// Duck: specific forms only — "ande" alone matches "koriander"/"mandel"
			"andesteg", "andebryst", "andelår", "andekød", "andfedt",
			"gås", "hønsebryst", "suppehøne", "kyllingefilet", "kyllingevinger", "kyllingebryst",
			"kyllingeskrog", "kalkunbryst", "salsiccia", "onglet", "kråse",
			"flæskesteg", "hakket kalve", "kødboller", "culotte", "roastbeef",
			"oksefilet", "oksehjørne", "oksebov", "oksehaler", "flæsk", "vildt", "hjort", "hare",
			"lammekølle", "lammebov", "lammefilet", "svinekrone",
		]),
		(IngredientCategory.Dairy, [
			"mælk", "fløde", "smør", "ost", "yogurt", "yoghurt", "kvark", "creme fraiche",
			"mozzarella", "parmesan", "ricotta", "kefir", "mascarpone", "brie", "gouda",
			"emmentaler", "havarti", "cottage", "fromage", "feta", "gorgonzola", "camembert",
			"hytteost", "rygeost", "blåskimmelost", "comté", "cheddar", "vesterhavsost",
			"gedeost", "flødeost", "kærnemælk", "tykmælk", "sødmælk", "skyr", "halloumi",
			"parmesanskorpe", "pecorino",
		]),
		(IngredientCategory.Legumes, [
			"linser", "kikærter", "sojabønner", "tofu", "tempeh", "edamamebønner", "edamame",
			"belugalinser", "beluga", "le puy linser",
		]),
		(IngredientCategory.NutsAndSeeds, [
			"nødder", "mandler", "valnødder", "hasselnødder", "cashew", "pistaciekerner",
			"pistacienødder", "pekannødder", "paranødder", "solsikkekerner", "græskarkerner",
			"sesamfrø", "chiafrø", "hørfrø", "valmuefrø", "pinjekerner", "kokosmasse",
			"jordnødder", "peanut", "peanutbutter", "mandelsmør", "cashewnøddesmør",
			"hampefrø", "nigella", "hirsefrø", "birkes", "blå birkes",
		]),
		(IngredientCategory.HerbsAndSpices, [
			"basilikum", "persille", "timian", "oregano", "koriander", "mynte", "dild",
			"rosmarin", "salvie", "estragon", "laurbær", "paprika", "karry", "gurkemeje",
			"kanel", "kardemomme", "nellike", "muskatnød", "allehånde", "spidskommen",
			"fennikelfrø", "anisfrø", "sennepsfrø", "ingefær", "wasabi", "sumak", "chiliflager",
			"peberkorn", "cayenne", "cajun", "krydderi", "lakridsrod", "lakridsrodspulver",
			"rålakridspulver", "citrongræs", "kaffirblade", "galangarod", "peberrod",
			"havtorn", "karse", "purløg", "brøndkarse", "sichuan peber", "rosa peberkorn",
			"citronmelisse", "morgenfrueblade", "hyldeblomst",
		]),
		(IngredientCategory.OilsAndFats, [
			"olivenolie", "rapsolie", "kokosolie", "svinefedt", "margarine", "ghee",
			"sesamolie", "palmeolie", "solsikkeolie", "fritureolie", "trøffelolie",
		]),
		(IngredientCategory.Sweeteners, [
			"sukker", "honning", "melasse", "agavesirup", "stevia", "flormelis",
			"ahornsirup", "rørsukker", "farin", "kandis", "muscovadosukker", "perlesukker",
			"vaniljesukker", "vaniljepulver", "vaniljestang",
			"karamel", "saltkaramel", "glasur", "fondant",
			"chokolade", "kakao",
		]),
		(IngredientCategory.Condiments, [
			// Oliven here — prevents "vin" (Beverages) from matching "oliven"
			"oliven", "oliventapenade",
			"salt", "sennep", "ketchup", "mayonnaise", "remoulade", "worcestershire",
			"worcester", "tabasco", "hoisin", "eddike", "tapenade", "pesto", "sambal",
			"sriracha", "aioli", "tahini", "miso", "soja", "sojasauce", "riseddike",
			"hvidvinseddike", "balsamico", "lagereddike", "æblecidereddike", "chutney",
			"chilisauce", "syltetøj", "marmelade", "pickles", "cornichoner",
			"chilipasta", "karrypasta", "saucejævner",
		]),
		(IngredientCategory.Beverages, [
			// Specific wine/alcohol words — avoids "vin" matching "oliven", "svine", etc.
			"hvidvin", "rødvin", "portvin", "rosévin", "hvidtøl", "champagne", "prosecco",
			"cointreau", "tequila", "vodka", "rom ", "mørk rom", "whiskey", "whisky",
			"likør", "baileys", "limoncello", "grand marnier", "madeira", "kahlua", "cachaca",
			"campari", "marsala", "amaretto", "snaps",
			"juice", "øl ", "kaffe", " te", "te ", "fond", "bouillon", "kokosmælk",
			"ribssaft", "jordbærsaft", "hyldebærsaft", "ananasjuice",
			"rabarbersaft", "danskvand", "sodavand", "kirsebærvin", "kirsebærlikør",
			"ingefærsirup", "lakridssirup", "hindbærsirup", "æblemost",
			"hønsefond", "kyllingefond", "oksebouillon", "kalvebouillon", "hønsebouillon",
			"grøntsagsbouillon", "fiskefond",
		]),
		(IngredientCategory.Bread, [
			"brød", "bolle", "toast", "baguette", "ciabatta", "pita", "tortilla",
			"knækbrød", "rugbrød", "pitabrød", "chapati", "naan", "wrap", "burgerboller",
			"hotdogbrød", "fladbrød", "bagels", "toastbrød", "forårsrulleplader",
			"surdej", "butterdej",
		]),
		(IngredientCategory.Grains, [
			// Specific flour types — avoids bare "mel" matching "melon", "marmelade", "karamel"
			"hvedemel", "rugmel", "speltmel", "durummel", "havremel", "grahamsmel", "bygmel",
			"mandelmel", "kokosmel", "kartoffelmel", "majsstivelse", "tipo 00",
			"ølandshvede", " mel", // " mel" with leading space catches "durum mel", "tipo 00 mel" etc.
			"ris", "pasta", "havregryn", "couscous", "quinoa", "bulgur", "spelt",
			"semolina", "polenta", "noodles", "spaghetti", "penne", "fusilli", "rigatoni",
			"lasagne", "tagliatelle", "pappardelle", "linguine", "gnocchi",
			"havregrød", "nudler", "jasminris", "risottoris", "perlebyg", "bygkerner",
			"boghvedegryn", "speltkerner", "lasagneplader", "pastaskruer",
			"rasp", "pankorasp", "natron", "bagepulver", "gær",
		]),
		(IngredientCategory.CannedGoods, [
			"dåse", "konserves",
		]),
	];

	private static readonly string[] GlutenKeywords =
	[
		// Specific flour types to avoid false positives on "melon", "marmelade", "karamel"
		"hvedemel", "rugmel", "speltmel", "durummel", "havremel", "grahamsmel", "bygmel",
		"tipo 00", "ølandshvede",
		"pasta", "brød", "bolle", "toast", "baguette", "ciabatta",
		"knækbrød", "tortilla", "naan", "chapati", "couscous", "bulgur",
		"havregryn", "rasp", "pankorasp", "lasagne", "noodles", "spaghetti",
		"penne", "fusilli", "rigatoni", "tagliatelle", "pappardelle", "linguine",
		"rugbrød", "bagels", "fladbrød", "forårsrulleplader",
		"surdej",
	];

	private static readonly string[] LactoseKeywords =
	[
		"mælk", "fløde", "smør", "ost", "yogurt", "yoghurt", "kvark", "mozzarella",
		"parmesan", "ricotta", "mascarpone", "kefir", "brie", "gouda", "emmentaler",
		"havarti", "cottage", "fromage", "feta", "gorgonzola", "camembert", "hytteost",
		"rygeost", "blåskimmelost", "comté", "cheddar", "vesterhavsost", "gedeost",
		"flødeost", "kærnemælk", "tykmælk", "sødmælk", "skyr", "halloumi", "creme fraiche",
		"pecorino",
	];

	private static readonly string[] NutKeywords =
	[
		"nødder", "mandler", "valnødder", "hasselnødder", "cashew", "pistaciekerner",
		"pistacienødder", "pekannødder", "paranødder", "pinjekerner", "kokosmasse",
		"jordnødder", "peanut",
	];

	private static readonly HashSet<IngredientCategory> NonVeganCategories = new()
	{
		IngredientCategory.Meat,
		IngredientCategory.Fish,
		IngredientCategory.Dairy,
		IngredientCategory.Eggs,
	};

	// Names that are classified as non-vegan regardless of category
	private static readonly string[] NonVeganKeywords =
	[
		"bouillon", "fond", "hønsefond", "kyllingefond", "oksebouillon", "kalvebouillon",
		"hønsebouillon", "grøntsagsbouillon" // grøntsagsbouillon is actually vegan, but better to be conservative
	];

	// Ingredients that are always vegan regardless of keywords (plant-based milks, etc.)
	private static readonly string[] VeganOverrideKeywords =
	[
		"mandelmælk", "havremælk", "sojamælk", "kokosmælk", "rismælk",
	];

	/// <summary>
	/// Assigns a category based on keyword matching in the ingredient name (lowercase Danish).
	/// Returns IngredientCategory.Other if no match is found.
	/// </summary>
	public static IngredientCategory Classify(string name)
	{
		var lower = name.ToLowerInvariant();

		foreach (var (substring, category) in PriorityOverrides)
			if (lower.Contains(substring))
				return category;

		foreach (var (category, keywords) in CategoryRules)
			foreach (var keyword in keywords)
				if (lower.Contains(keyword))
					return category;

		return IngredientCategory.Other;
	}

	/// <summary>
	/// Detects common allergens and vegan status from the ingredient name.
	/// </summary>
	public static (bool ContainsGluten, bool ContainsLactose, bool ContainsNuts, bool IsVegan) DetectAllergens(string name)
	{
		var lower = name.ToLowerInvariant();

		// Check vegan overrides first to avoid contradictions (e.g., plant-based milks)
		var isVeganOverride = VeganOverrideKeywords.Any(lower.Contains);

		var containsGluten = GlutenKeywords.Any(lower.Contains);
		// Plant-based milks should NOT be flagged as containing lactose
		var containsLactose = !isVeganOverride && LactoseKeywords.Any(lower.Contains);
		var containsNuts = NutKeywords.Any(lower.Contains);

		bool isVegan;
		if (isVeganOverride)
		{
			// Plant-based milks etc. are explicitly vegan
			isVegan = true;
		}
		else
		{
			var category = Classify(name);
			isVegan = !NonVeganCategories.Contains(category)
				&& !containsLactose
				&& !NonVeganKeywords.Any(lower.Contains);
		}

		return (containsGluten, containsLactose, containsNuts, isVegan);
	}

	/// <summary>
	/// Returns the typical store-purchase unit for an ingredient based on its category and name.
	/// </summary>
	public static string GetDefaultUnit(string name, IngredientCategory category)
	{
		var lower = name.ToLowerInvariant();

		return category switch
		{
			IngredientCategory.Meat => "g",
			IngredientCategory.Fish => "g",
			IngredientCategory.Eggs => "stk",
			IngredientCategory.Grains => "g",
			IngredientCategory.NutsAndSeeds => "g",
			IngredientCategory.Sweeteners => ContainsAny(lower, "sirup", "honning", "ahornsirup", "melasse", "agavesirup") ? "ml" : "g",
			IngredientCategory.Beverages => "ml",
			IngredientCategory.CannedGoods => "g",
			IngredientCategory.Legumes => "g",
			IngredientCategory.HerbsAndSpices => "g",
			IngredientCategory.Condiments => ContainsAny(lower, "sauce", "soja", "eddike", "worcester", "tabasco", "ketchup", "sirup", "olie", "bouillon") ? "ml" : "g",
			IngredientCategory.OilsAndFats => ContainsAny(lower, "smør", "margarine", "svinefedt", "ghee") ? "g" : "ml",
			IngredientCategory.Dairy => ContainsAny(lower, "mælk", "fløde", "yogurt", "yoghurt", "kefir", "kærnemælk", "tykmælk", "sødmælk", "skyr") ? "ml" : "g",
			IngredientCategory.Bread => ContainsAny(lower, "rugbrød", "ciabatta", "baguette", "knækbrød", "fladbrød") ? "g" : "stk",
			IngredientCategory.Vegetables => ContainsAny(lower, "løg", "hvidløg", "peberfrugt", "agurk", "tomat", "kartoffel", "avocado", "chili", "aubergine", "squash", "zucchini", "fennikel") ? "stk" : "g",
			IngredientCategory.Fruits => ContainsAny(lower, "saft", "juice", "mos", "most") ? "ml"
				: ContainsAny(lower, "banan", "æble", "pære", "appelsin", "citron", "lime", "mango", "kiwi", "fersken", "blomme", "blodappelsin", "nektarin", "avocado", "passionsfrugt") ? "stk" : "g",
			_ => "stk",
		};
	}

	private static bool ContainsAny(string text, params string[] keywords)
		=> keywords.Any(text.Contains);
}
