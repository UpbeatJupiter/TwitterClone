using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Twitter.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntityNamesModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Followed_Users_UserId",
                table: "Followed");

            migrationBuilder.RenameColumn(
                name: "UserFollowedBy",
                table: "Followed",
                newName: "FollowingUserId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Followed",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FollowedUserId",
                table: "Followed",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Followed_Users_UserId",
                table: "Followed",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Followed_Users_UserId",
                table: "Followed");

            migrationBuilder.DropColumn(
                name: "FollowedUserId",
                table: "Followed");

            migrationBuilder.RenameColumn(
                name: "FollowingUserId",
                table: "Followed",
                newName: "UserFollowedBy");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Followed",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Followed_Users_UserId",
                table: "Followed",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
