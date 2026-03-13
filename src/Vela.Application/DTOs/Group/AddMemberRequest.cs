namespace Vela.Application.DTOs.Group;

public class AddMemberRequest
{
    public required string UserId { get; set; }
    public string? Role { get; set; }
}