namespace Vela.Application.DTOs;

public class CreateShoppingListDto
{
    public required string Name { get; set; }
    public Guid? GroupId { get; set; }
}