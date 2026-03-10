﻿namespace Vela.Application.DTOs.MealPlan;

public class MealPlanEntryDto
{
    public Guid Id { get; set; }
    public Guid MealPlanId { get; set; }
    public Guid RecipeId { get; set; }
    public required DateOnly Date { get; set; }
    public required string MealType { get; set; }
    public int Servings { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public RecipeSummaryDto? Recipe { get; set; }
}
