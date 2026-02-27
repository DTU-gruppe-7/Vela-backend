namespace Vela.Application.DTOs;

public class RecipeSummaryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? TotalTime { get; set; }
}
