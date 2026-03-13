namespace Vela.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid RecipeId { get; set; }
    public DateTimeOffset MatchedAt { get; set; } = DateTimeOffset.UtcNow;
}