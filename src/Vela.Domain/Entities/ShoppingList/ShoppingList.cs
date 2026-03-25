namespace Vela.Domain.Entities.ShoppingList;

public class ShoppingList
{
    public Guid Id { get; set; }
    
    public string? UserId { get; set; }
    public Guid? GroupId { get; set; }
    
    public string? Name { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<ShoppingListItem>? Items { get; set; } = new();

}