
namespace Vela.Application.DTOs;

public class RecipeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int ServingSize { get; set; }
    public string? TotalTime { get; set; }   // f.eks. "PT20M"
    public string? WorkTime { get; set; }     // f.eks. "PT20M"
    public string? InstructionsJson { get; set; } // Gemmer instruktioner som JSON
    public string? KeywordsJson { get; set; }     // Gemmer nøgleord som JSON

    public required List<RecipeIngredientDto> Ingredients { get; set; }
}