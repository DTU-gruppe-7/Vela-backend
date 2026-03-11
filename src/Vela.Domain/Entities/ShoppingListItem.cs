namespace Vela.Domain.Entities;

public class ShoppingListItem
{
    public Guid Id  { get; set; }
    public Guid ShoppingListId { get; set; }
    public virtual required ShoppingList ShoppingList { get; set; }
    
    public Guid IngredientId { get; set; }
    public virtual required Ingredient Ingredient { get; set; }
    
    public string? AssignedUserId { get; set; }
    
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    
    public decimal? Price { get; set; }
    public string? Shop { get; set; }
    public bool IsBought { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
}