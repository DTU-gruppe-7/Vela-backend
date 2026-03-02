
namespace Vela.Domain.Entities
{

	public class Recipe
	{
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public string? Description { get; set; }
		public string? Category { get; set; }
		public string? ThumbnailUrl { get; set; }
		public string? SourceUrl { get; set; }
		public int ServingSize { get; set; }
		public string? TotalTime { get; set; }   // f.eks. "PT20M"
		public string? WorkTime { get; set; }     // f.eks. "PT20M"
		public string? InstructionsJson { get; set; } // Gemmer instruktioner som JSON
		public string? KeywordsJson { get; set; }     // Gemmer nøgleord som JSON

		public List<RecipeIngredient> Ingredients { get; set; } = new();
	}
}
