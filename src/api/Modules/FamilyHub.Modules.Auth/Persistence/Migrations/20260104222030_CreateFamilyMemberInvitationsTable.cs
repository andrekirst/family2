using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateFamilyMemberInvitationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "full_name",
                schema: "auth",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                schema: "auth",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zitadel_user_id",
                schema: "auth",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "family_member_invitations",
                schema: "auth",
                columns: table => new
                {
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.CheckConstraint("ck_family_member_invitations_email_xor_username", "(email IS NOT NULL AND username IS NULL) OR (email IS NULL AND username IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_family_member_invitations_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "auth",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_family_member_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "auth",
                table: "users",
                column: "username",
                unique: true,
                filter: "username IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_zitadel_user_id",
                schema: "auth",
                table: "users",
                column: "zitadel_user_id",
                filter: "zitadel_user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_expires_at",
                schema: "auth",
                table: "family_member_invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_family_id",
                schema: "auth",
                table: "family_member_invitations",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_family_id_status",
                schema: "auth",
                table: "family_member_invitations",
                columns: new[] { "family_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_invited_by_user_id",
                schema: "auth",
                table: "family_member_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_invitations_token",
                schema: "auth",
                table: "family_member_invitations",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_member_invitations",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                schema: "auth",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_zitadel_user_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "full_name",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "username",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "zitadel_user_id",
                schema: "auth",
                table: "users");
        }
    }
}
