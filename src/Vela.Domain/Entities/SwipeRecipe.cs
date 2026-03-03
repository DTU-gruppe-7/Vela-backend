using Vela.Domain.Enums;

namespace Vela.Domain.Entities;

public class SwipeRecipe
{
 
    public Guid SwipeId { get; set; }
    public Guid UserId { get; set; }
    public Guid RecipeId { get; set; }
    public SwipeDirection Direction { get; set; } // Like/Dislike
    public DateTimeOffset SwipedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    //public virtual User User { get; set; } Denne skal først bruges når vi får en authentication system op at køre, så vi kan linke swipes til brugere
    public virtual Recipe Recipe { get; set; }

}