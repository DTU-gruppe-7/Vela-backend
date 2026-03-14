using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Match : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SwipeRecipes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_GroupId_RecipeId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GroupInvites");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GroupMembers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                columns: new[] { "GroupId", "RecipeId" });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    LikeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    SwipedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_Likes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_RecipeId",
                table: "Likes",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId_RecipeId",
                table: "Likes",
                columns: new[] { "UserId", "RecipeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GroupMembers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "GroupInvites",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SwipeRecipes",
                columns: table => new
                {
                    SwipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    SwipedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwipeRecipes", x => x.SwipeId);
                    table.ForeignKey(
                        name: "FK_SwipeRecipes_Recipes_RecipeId",
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
                name: "IX_SwipeRecipes_RecipeId",
                table: "SwipeRecipes",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_SwipeRecipes_UserId_RecipeId",
                table: "SwipeRecipes",
                columns: new[] { "UserId", "RecipeId" },
                unique: true);
        }
    }
}
