using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zajednica.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "community");

            migrationBuilder.CreateTable(
                name: "BlacklistEntries",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuerMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificationChallenges",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuerMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Communities",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StreetName = table.Column<string>(type: "text", nullable: false),
                    StreetNumber = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: true),
                    QrToken = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedByMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitNumber = table.Column<string>(type: "text", nullable: true),
                    CertificationStatus = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MutedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Stars = table.Column<int>(type: "integer", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipRoles",
                schema: "community",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrantedByMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipRoles_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalSchema: "community",
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistEntries_AccountId_CommunityId",
                schema: "community",
                table: "BlacklistEntries",
                columns: new[] { "AccountId", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CommunityId",
                schema: "community",
                table: "Certificates",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_IssuerMembershipId_CandidateMembershipId",
                schema: "community",
                table: "Certificates",
                columns: new[] { "IssuerMembershipId", "CandidateMembershipId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificationChallenges_Token",
                schema: "community",
                table: "CertificationChallenges",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communities_QrToken",
                schema: "community",
                table: "Communities",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CommunityId",
                schema: "community",
                table: "Documents",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRoles_MembershipId_Role",
                schema: "community",
                table: "MembershipRoles",
                columns: new[] { "MembershipId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_AccountId_CommunityId",
                schema: "community",
                table: "Memberships",
                columns: new[] { "AccountId", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_CommunityId",
                schema: "community",
                table: "Memberships",
                column: "CommunityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistEntries",
                schema: "community");

            migrationBuilder.DropTable(
                name: "Certificates",
                schema: "community");

            migrationBuilder.DropTable(
                name: "CertificationChallenges",
                schema: "community");

            migrationBuilder.DropTable(
                name: "Communities",
                schema: "community");

            migrationBuilder.DropTable(
                name: "Documents",
                schema: "community");

            migrationBuilder.DropTable(
                name: "MembershipRoles",
                schema: "community");

            migrationBuilder.DropTable(
                name: "Memberships",
                schema: "community");
        }
    }
}
