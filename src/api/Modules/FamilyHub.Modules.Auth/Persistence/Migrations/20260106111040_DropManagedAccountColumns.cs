using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropManagedAccountColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "queued_managed_account_creations",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "ix_users_real_email",
                schema: "auth",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                schema: "auth",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_zitadel_user_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "real_email",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "real_email_verified",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "username",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "username_login_enabled",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "zitadel_user_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "auth",
                table: "family_member_invitations");

            migrationBuilder.DropColumn(
                name: "username",
                schema: "auth",
                table: "family_member_invitations");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "auth",
                table: "family_member_invitations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "auth",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "real_email",
                schema: "auth",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "real_email_verified",
                schema: "auth",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "username",
                schema: "auth",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "username_login_enabled",
                schema: "auth",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "zitadel_user_id",
                schema: "auth",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "auth",
                table: "family_member_invitations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "auth",
                table: "family_member_invitations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                schema: "auth",
                table: "family_member_invitations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "queued_managed_account_creations",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    encrypted_password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    next_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_queued_managed_account_creations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_real_email",
                schema: "auth",
                table: "users",
                column: "real_email",
                unique: true,
                filter: "real_email IS NOT NULL");

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
                name: "ix_queued_managed_account_creations_family_id",
                schema: "auth",
                table: "queued_managed_account_creations",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_queued_managed_account_creations_status",
                schema: "auth",
                table: "queued_managed_account_creations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_queued_managed_account_creations_status_next_retry",
                schema: "auth",
                table: "queued_managed_account_creations",
                columns: new[] { "status", "next_retry_at" });
        }
    }
}
