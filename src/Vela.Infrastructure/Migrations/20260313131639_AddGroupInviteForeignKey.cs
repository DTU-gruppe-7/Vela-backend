using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupInviteForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_GroupInvites_Groups_GroupId",
                table: "GroupInvites",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupInvites_Groups_GroupId",
                table: "GroupInvites");
        }
    }
}
