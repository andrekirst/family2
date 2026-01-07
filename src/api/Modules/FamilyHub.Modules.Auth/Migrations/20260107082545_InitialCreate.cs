using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "families",
                schema: "auth",
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
                name: "outbox_events",
                schema: "auth",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    event_version = table.Column<int>(type: "integer", nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_events", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    external_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "auth",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "family_member_invitations",
                schema: "auth",
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

            migrationBuilder.CreateIndex(
                name: "ix_outbox_events_created_at",
                schema: "auth",
                table: "outbox_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_events_status_created_at",
                schema: "auth",
                table: "outbox_events",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_external_provider_user_id",
                schema: "auth",
                table: "users",
                columns: new[] { "external_provider", "external_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_family_id",
                schema: "auth",
                table: "users",
                column: "family_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_member_invitations",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "outbox_events",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "families",
                schema: "auth");
        }
    }
}
