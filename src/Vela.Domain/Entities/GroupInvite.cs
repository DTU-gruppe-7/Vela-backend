namespace Vela.Domain.Entities;

public class GroupInvite
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}