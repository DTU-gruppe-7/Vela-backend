using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class bugfixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlans_AspNetUsers_AppUserId",
                table: "MealPlans");

            migrationBuilder.DropIndex(
                name: "IX_MealPlans_AppUserId",
                table: "MealPlans");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "MealPlans");

            migrationBuilder.AddColumn<bool>(
                name: "AddedToShoppingList",
                table: "MealPlanEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "MealPlanEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedToShoppingList",
                table: "MealPlanEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MealPlanEntries");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "MealPlans",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_AppUserId",
                table: "MealPlans",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlans_AspNetUsers_AppUserId",
                table: "MealPlans",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
