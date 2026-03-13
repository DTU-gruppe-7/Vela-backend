namespace Vela.Application.DTOs.Group;

public class GroupInviteDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}