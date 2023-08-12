using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Twitter.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserTweetDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTweets");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_UserId",
                table: "Tweets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_Users_UserId",
                table: "Tweets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_Users_UserId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_Tweets_UserId",
                table: "Tweets");

            migrationBuilder.CreateTable(
                name: "UserTweets",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TweetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTweets", x => new { x.UserId, x.TweetId });
                    table.ForeignKey(
                        name: "FK_UserTweets_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTweets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTweets_TweetId",
                table: "UserTweets",
                column: "TweetId");
        }
    }
}
