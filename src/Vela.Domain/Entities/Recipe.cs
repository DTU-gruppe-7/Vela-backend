﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Vela.Domain.Entities
{

	public class Recipe
	{
			public Guid Id { get; set; }
			public string ExternalId { get; set; }
			public string Name { get; set; }
			public string Instructions { get; set; }
		    public string Category { get; set; }
		    public string ThumbnailUrl { get; set; }

			public List<RecipeIngredient> Ingredients { get; set; } = new();
			//public List<Rating> Ratings { get; set; } = new();
    }
}
