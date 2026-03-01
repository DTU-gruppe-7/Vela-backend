namespace Vela.Application.DTOs;

public class RecipeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? WorkTime { get; set; }
    public string? TotalTime { get; set; }
    public string? KeywordsJson { get; set; }
}
