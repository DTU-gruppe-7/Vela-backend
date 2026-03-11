namespace Vela.Application.DTOs;

public class CreateShoppingListDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? GroupId { get; set; } 
}