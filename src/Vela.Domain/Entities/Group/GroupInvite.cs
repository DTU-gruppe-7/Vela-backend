namespace Vela.Domain.Entities.Group;

public class GroupInvite
{
    public required Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}