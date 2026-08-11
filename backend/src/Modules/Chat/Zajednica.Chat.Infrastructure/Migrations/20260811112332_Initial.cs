using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zajednica.Chat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.CreateTable(
                name: "Chats",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatParticipants",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true),
                    LastReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatParticipants_Chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DirectChats",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectChats_Chats_Id",
                        column: x => x.Id,
                        principalSchema: "chat",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HelpRequestChats",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HelpRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FirstResponse = table.Column<bool>(type: "boolean", nullable: false),
                    AwardedStars = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpRequestChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpRequestChats_Chats_Id",
                        column: x => x.Id,
                        principalSchema: "chat",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryChats",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemporaryChats_Chats_Id",
                        column: x => x.Id,
                        principalSchema: "chat",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextMessages",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextMessages_Messages_Id",
                        column: x => x.Id,
                        principalSchema: "chat",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiceMessages",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceMessages_Messages_Id",
                        column: x => x.Id,
                        principalSchema: "chat",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipants_ChatId",
                schema: "chat",
                table: "ChatParticipants",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipants_MembershipId",
                schema: "chat",
                table: "ChatParticipants",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_CommunityId_LastActivityAt",
                schema: "chat",
                table: "Chats",
                columns: new[] { "CommunityId", "LastActivityAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HelpRequestChats_HelpRequestId",
                schema: "chat",
                table: "HelpRequestChats",
                column: "HelpRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId_Date",
                schema: "chat",
                table: "Messages",
                columns: new[] { "ChatId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatParticipants",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "DirectChats",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "HelpRequestChats",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "TemporaryChats",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "TextMessages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "VoiceMessages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "Chats",
                schema: "chat");
        }
    }
}
