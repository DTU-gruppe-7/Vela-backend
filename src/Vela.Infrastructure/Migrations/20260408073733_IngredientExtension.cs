using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IngredientExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IngredientId",
                table: "ShoppingListItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemCategory",
                table: "ShoppingListItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "MealPlanEntryId",
                table: "ShoppingListItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Ingredients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Ingredients",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_MealPlanEntryId",
                table: "ShoppingListItems",
                column: "MealPlanEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_MealPlanEntries_MealPlanEntryId",
                table: "ShoppingListItems",
                column: "MealPlanEntryId",
                principalTable: "MealPlanEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_MealPlanEntries_MealPlanEntryId",
                table: "ShoppingListItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingListItems_MealPlanEntryId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "IngredientId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "ItemCategory",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "MealPlanEntryId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Ingredients");
        }
    }
}
