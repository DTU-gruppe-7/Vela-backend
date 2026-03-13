namespace Vela.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Status { get; set; }
    public List<GroupMember> Members { get; set; } = new();
    public List<Match> Matches { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
