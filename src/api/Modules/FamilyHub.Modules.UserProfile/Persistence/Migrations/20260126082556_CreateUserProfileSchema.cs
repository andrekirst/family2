using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.UserProfile.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserProfileSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user_profile");

            migrationBuilder.CreateTable(
                name: "profiles",
                schema: "user_profile",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    pronouns = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "UTC"),
                    date_format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "yyyy-MM-dd"),
                    birthday_visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "family"),
                    pronouns_visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "family"),
                    preferences_visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "hidden"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profiles_user_id",
                schema: "user_profile",
                table: "profiles",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profiles",
                schema: "user_profile");
        }
    }
}
