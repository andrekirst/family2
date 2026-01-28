using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.UserProfile.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_synced_at",
                schema: "user_profile",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "sync_status",
                schema: "user_profile",
                table: "profiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_synced_at",
                schema: "user_profile",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sync_status",
                schema: "user_profile",
                table: "profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending");
        }
    }
}
