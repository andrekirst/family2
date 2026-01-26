using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.UserProfile.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileChangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profile_change_requests",
                schema: "user_profile",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    old_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    new_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile_change_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profile_change_requests_family_id_status",
                schema: "user_profile",
                table: "profile_change_requests",
                columns: new[] { "family_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_profile_change_requests_profile_id_field_name_status",
                schema: "user_profile",
                table: "profile_change_requests",
                columns: new[] { "profile_id", "field_name", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_profile_change_requests_requested_by_status",
                schema: "user_profile",
                table: "profile_change_requests",
                columns: new[] { "requested_by", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile_change_requests",
                schema: "user_profile");
        }
    }
}
