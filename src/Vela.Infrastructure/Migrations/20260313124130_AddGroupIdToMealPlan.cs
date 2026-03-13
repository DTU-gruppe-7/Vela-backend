using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdToMealPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "MealPlans",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "MealPlans");
        }
    }
}
