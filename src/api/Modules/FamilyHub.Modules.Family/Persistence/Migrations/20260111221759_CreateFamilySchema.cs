using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Family.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateFamilySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "family");

            migrationBuilder.CreateTable(
                name: "families",
                schema: "family",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "family_member_invitations",
                schema: "family",
                columns: table => new
                {
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_member_invitations", x => x.invitation_id);
                    table.ForeignKey(
                        name: "fk_family_member_invitations_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_families_owner_id",
                schema: "family",
                table: "families",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_expires_at",
                schema: "family",
                table: "family_member_invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_family_id",
                schema: "family",
                table: "family_member_invitations",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_family_id_status",
                schema: "family",
                table: "family_member_invitations",
                columns: new[] { "family_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_invited_by_user_id",
                schema: "family",
                table: "family_member_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_token",
                schema: "family",
                table: "family_member_invitations",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_member_invitations",
                schema: "family");

            migrationBuilder.DropTable(
                name: "families",
                schema: "family");
        }
    }
}
