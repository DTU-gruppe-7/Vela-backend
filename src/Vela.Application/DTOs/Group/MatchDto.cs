using Vela.Application.DTOs;

namespace Vela.Application.DTOs.Group;

public class MatchDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid RecipeId { get; set; }
    public RecipeSummaryDto? Recipe { get; set; }
    public DateTimeOffset MatchedAt { get; set; }
}