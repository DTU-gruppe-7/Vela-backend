namespace Vela.Application.DTOs.ShoppingList;

public class CreateShoppingListDto
{
    public required string Name { get; set; }
    public Guid? GroupId { get; set; }
}