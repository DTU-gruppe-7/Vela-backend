using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editShoppingList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_Ingredients_IngredientId",
                table: "ShoppingListItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingListItems_IngredientId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "IngredientId",
                table: "ShoppingListItems");

            migrationBuilder.AddColumn<string>(
                name: "IngredientName",
                table: "ShoppingListItems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientName",
                table: "ShoppingListItems");

            migrationBuilder.AddColumn<Guid>(
                name: "IngredientId",
                table: "ShoppingListItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_IngredientId",
                table: "ShoppingListItems",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_Ingredients_IngredientId",
                table: "ShoppingListItems",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
