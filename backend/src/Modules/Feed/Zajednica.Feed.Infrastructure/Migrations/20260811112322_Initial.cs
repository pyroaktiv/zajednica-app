using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zajednica.Feed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "feed");

            migrationBuilder.CreateTable(
                name: "IntentEvents",
                schema: "feed",
                columns: table => new
                {
                    StreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Kind = table.Column<string>(type: "text", nullable: true),
                    TargetMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetMembershipStatus = table.Column<string>(type: "text", nullable: true),
                    TargetMembershipRole = table.Column<string>(type: "text", nullable: true),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EligibleVoterCount = table.Column<int>(type: "integer", nullable: true),
                    VoterMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    InFavor = table.Column<bool>(type: "boolean", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentEvents", x => new { x.StreamId, x.Sequence });
                });

            migrationBuilder.CreateTable(
                name: "IntentViews",
                schema: "feed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfClosure = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    VotesAreVisible = table.Column<bool>(type: "boolean", nullable: false),
                    EligibleVoterCount = table.Column<int>(type: "integer", nullable: false),
                    VotesFor = table.Column<int>(type: "integer", nullable: false),
                    VotesAgainst = table.Column<int>(type: "integer", nullable: false),
                    QuorumReached = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentViews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "feed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "feed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    HasReplies = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalSchema: "feed",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "feed",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneralTopicPosts",
                schema: "feed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralTopicPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralTopicPosts_Posts_Id",
                        column: x => x.Id,
                        principalSchema: "feed",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HelpRequests",
                schema: "feed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Closed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpRequests_Posts_Id",
                        column: x => x.Id,
                        principalSchema: "feed",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                schema: "feed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "feed",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "feed",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId_ParentCommentId_Date",
                schema: "feed",
                table: "Comments",
                columns: new[] { "PostId", "ParentCommentId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Images_PostId",
                schema: "feed",
                table: "Images",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_IntentViews_CommunityId_DateCreated",
                schema: "feed",
                table: "IntentViews",
                columns: new[] { "CommunityId", "DateCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_IntentViews_CommunityId_TargetMembershipId",
                schema: "feed",
                table: "IntentViews",
                columns: new[] { "CommunityId", "TargetMembershipId" },
                filter: "\"Status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "IX_IntentViews_Deadline",
                schema: "feed",
                table: "IntentViews",
                column: "Deadline",
                filter: "\"Status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CommunityId_DateCreated",
                schema: "feed",
                table: "Posts",
                columns: new[] { "CommunityId", "DateCreated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments",
                schema: "feed");

            migrationBuilder.DropTable(
                name: "GeneralTopicPosts",
                schema: "feed");

            migrationBuilder.DropTable(
                name: "HelpRequests",
                schema: "feed");

            migrationBuilder.DropTable(
                name: "Images",
                schema: "feed");

            migrationBuilder.DropTable(
                name: "IntentEvents",
                schema: "feed");

            migrationBuilder.DropTable(
                name: "IntentViews",
                schema: "feed");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "feed");
        }
    }
}
