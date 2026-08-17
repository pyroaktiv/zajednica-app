using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zajednica.Feed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PostId",
                schema: "feed",
                table: "IntentViews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostId",
                schema: "feed",
                table: "IntentEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommunityRatings",
                schema: "feed",
                columns: table => new
                {
                    GeneralTopicPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalPercentage = table.Column<int>(type: "integer", nullable: false),
                    Zone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityRatings", x => x.GeneralTopicPostId);
                    table.ForeignKey(
                        name: "FK_CommunityRatings_GeneralTopicPosts_GeneralTopicPostId",
                        column: x => x.GeneralTopicPostId,
                        principalSchema: "feed",
                        principalTable: "GeneralTopicPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntentViews_PostId",
                schema: "feed",
                table: "IntentViews",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityRatings",
                schema: "feed");

            migrationBuilder.DropIndex(
                name: "IX_IntentViews_PostId",
                schema: "feed",
                table: "IntentViews");

            migrationBuilder.DropColumn(
                name: "PostId",
                schema: "feed",
                table: "IntentViews");

            migrationBuilder.DropColumn(
                name: "PostId",
                schema: "feed",
                table: "IntentEvents");
        }
    }
}
