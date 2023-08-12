using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Twitter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicLink",
                table: "Tweets");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Tweets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tweets");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicLink",
                table: "Tweets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
