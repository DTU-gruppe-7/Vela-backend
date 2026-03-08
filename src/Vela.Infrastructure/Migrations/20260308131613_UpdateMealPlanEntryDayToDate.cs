using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMealPlanEntryDayToDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealPlanEntries_MealPlanId_Day_MealType",
                table: "MealPlanEntries");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "MealPlanEntries");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "MealPlanEntries",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntries_MealPlanId_Date_MealType",
                table: "MealPlanEntries",
                columns: new[] { "MealPlanId", "Date", "MealType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealPlanEntries_MealPlanId_Date_MealType",
                table: "MealPlanEntries");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "MealPlanEntries");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "MealPlanEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntries_MealPlanId_Day_MealType",
                table: "MealPlanEntries",
                columns: new[] { "MealPlanId", "Day", "MealType" });
        }
    }
}
