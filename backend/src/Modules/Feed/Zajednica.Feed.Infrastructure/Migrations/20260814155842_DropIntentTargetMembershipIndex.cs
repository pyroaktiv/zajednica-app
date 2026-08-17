using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zajednica.Feed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropIntentTargetMembershipIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IntentViews_CommunityId_TargetMembershipId",
                schema: "feed",
                table: "IntentViews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IntentViews_CommunityId_TargetMembershipId",
                schema: "feed",
                table: "IntentViews",
                columns: new[] { "CommunityId", "TargetMembershipId" },
                filter: "\"Status\" = 'Open'");
        }
    }
}
