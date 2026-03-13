using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGroupMatchToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupMatches");

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matches_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_GroupId_RecipeId",
                table: "Matches",
                columns: new[] { "GroupId", "RecipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RecipeId",
                table: "Matches",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.CreateTable(
                name: "GroupMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMatches_GroupId_RecipeId",
                table: "GroupMatches",
                columns: new[] { "GroupId", "RecipeId" },
                unique: true);
        }
    }
}
