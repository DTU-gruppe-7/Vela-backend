using Vela.Domain.Enums;

namespace Vela.Application.DTOs;

public class SwipeDto
{
    public Guid RecipeId { get; set; }
    public SwipeDirection Direction { get; set; }
}
