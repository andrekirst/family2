using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDualAuthenticationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "username_login_enabled",
                schema: "auth",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_real_email",
                schema: "auth",
                table: "users",
                column: "real_email",
                unique: true,
                filter: "real_email IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_real_email",
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
                name: "username_login_enabled",
                schema: "auth",
                table: "users");
        }
    }
}
