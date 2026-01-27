using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToLocalAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_families_family_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropTable(
                name: "family_member_invitations",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "families",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "ix_users_external_provider_user_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_provider",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_user_id",
                schema: "auth",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "email_verification_token",
                schema: "auth",
                table: "users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "email_verification_token_expires_at",
                schema: "auth",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                schema: "auth",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_end_time",
                schema: "auth",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                schema: "auth",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_reset_code",
                schema: "auth",
                table: "users",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_reset_code_expires_at",
                schema: "auth",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_reset_token",
                schema: "auth",
                table: "users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_reset_token_expires_at",
                schema: "auth",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "auth_audit_logs",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auth_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_logins",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    provider_display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_logins", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_logins_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auth_audit_logs_event_type",
                schema: "auth",
                table: "auth_audit_logs",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_auth_audit_logs_occurred_at",
                schema: "auth",
                table: "auth_audit_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_auth_audit_logs_user_id",
                schema: "auth",
                table: "auth_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_logins_provider_user_id",
                schema: "auth",
                table: "external_logins",
                columns: new[] { "provider", "provider_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_logins_user_id",
                schema: "auth",
                table: "external_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires_at",
                schema: "auth",
                table: "refresh_tokens",
                column: "expires_at",
                filter: "is_revoked = false");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                schema: "auth",
                table: "refresh_tokens",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                schema: "auth",
                table: "refresh_tokens",
                column: "user_id");

            // Create enhanced RLS function for family_id
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION auth.current_family_id() RETURNS UUID AS $$
                    SELECT NULLIF(current_setting('app.current_family_id', true), '')::UUID;
                $$ LANGUAGE SQL STABLE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop enhanced RLS function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS auth.current_family_id();");

            migrationBuilder.DropTable(
                name: "auth_audit_logs",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "external_logins",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "auth");

            migrationBuilder.DropColumn(
                name: "email_verification_token",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email_verification_token_expires_at",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_end_time",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_hash",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_code",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_code_expires_at",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_token",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_token_expires_at",
                schema: "auth",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "external_provider",
                schema: "auth",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "external_user_id",
                schema: "auth",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "families",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "family_member_invitations",
                schema: "auth",
                columns: table => new
                {
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    display_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_member_invitations", x => x.invitation_id);
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
                name: "ix_users_external_provider_user_id",
                schema: "auth",
                table: "users",
                columns: new[] { "external_provider", "external_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_families_owner_id",
                schema: "auth",
                table: "families",
                column: "owner_id");

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

            migrationBuilder.AddForeignKey(
                name: "fk_users_families_family_id",
                schema: "auth",
                table: "users",
                column: "family_id",
                principalSchema: "auth",
                principalTable: "families",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
