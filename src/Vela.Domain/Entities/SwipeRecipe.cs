using Vela.Domain.Enums;

namespace Vela.Domain.Entities;

public class SwipeRecipe
{
 
    public Guid SwipeId { get; set; }
    public required string UserId { get; set; }
    public Guid RecipeId { get; set; }
    public SwipeDirection Direction { get; set; } // Like/Dislike
    public DateTimeOffset SwipedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public virtual Recipe Recipe { get; set; }

}