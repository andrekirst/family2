using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "google_integration");

            migrationBuilder.CreateTable(
                name: "google_account_links",
                schema: "google_integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    google_account_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    google_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    encrypted_access_token = table.Column<string>(type: "text", nullable: false),
                    encrypted_refresh_token = table.Column<string>(type: "text", nullable: false),
                    access_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    granted_scopes = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_google_account_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oauth_states",
                schema: "google_integration",
                columns: table => new
                {
                    state = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_verifier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_oauth_states", x => x.state);
                });

            migrationBuilder.CreateIndex(
                name: "ix_google_account_links_google_account_id",
                schema: "google_integration",
                table: "google_account_links",
                column: "google_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_google_account_links_status",
                schema: "google_integration",
                table: "google_account_links",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_google_account_links_user_id",
                schema: "google_integration",
                table: "google_account_links",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_oauth_states_expires_at",
                schema: "google_integration",
                table: "oauth_states",
                column: "expires_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "google_account_links",
                schema: "google_integration");

            migrationBuilder.DropTable(
                name: "oauth_states",
                schema: "google_integration");
        }
    }
}
