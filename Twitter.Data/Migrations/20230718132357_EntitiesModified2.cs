using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Twitter.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntitiesModified2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Users_UserId",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_UserId",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Follows");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowedUserId",
                table: "Follows",
                column: "FollowedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Users_FollowedUserId",
                table: "Follows",
                column: "FollowedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Users_FollowedUserId",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_FollowedUserId",
                table: "Follows");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Follows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_UserId",
                table: "Follows",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Users_UserId",
                table: "Follows",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
