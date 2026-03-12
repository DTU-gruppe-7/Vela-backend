namespace Vela.Application.DTOs;

public class ShoppingListSummaryDto
{
    public Guid Id { get; set; }
    
    public string? UserId { get; set; }
    public Guid? GroupId { get; set; }
    
    public string? Name { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}