using Vela.Domain.Enums;

namespace Vela.Application.DTOs.Group;

public class GroupMemberDto
{
    public required Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public GroupRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}