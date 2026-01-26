using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.UserProfile.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profile_events",
                schema: "user_profile",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profile_events_occurred_at",
                schema: "user_profile",
                table: "profile_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_profile_events_profile_type",
                schema: "user_profile",
                table: "profile_events",
                columns: new[] { "profile_id", "event_type" });

            migrationBuilder.CreateIndex(
                name: "ix_profile_events_profile_version",
                schema: "user_profile",
                table: "profile_events",
                columns: new[] { "profile_id", "version" },
                unique: true);

            // Add foreign key constraint to profiles table with cascade delete
            migrationBuilder.AddForeignKey(
                name: "fk_profile_events_profiles",
                schema: "user_profile",
                table: "profile_events",
                column: "profile_id",
                principalSchema: "user_profile",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_profile_events_profiles",
                schema: "user_profile",
                table: "profile_events");

            migrationBuilder.DropTable(
                name: "profile_events",
                schema: "user_profile");
        }
    }
}
