using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCurrentFamilyToUserFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_current_family",
                schema: "auth",
                table: "user_families",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Data migration: Set is_current_family = true for owner memberships
            migrationBuilder.Sql(@"
                UPDATE auth.user_families
                SET is_current_family = true
                WHERE role = 'Owner';
            ");

            migrationBuilder.CreateIndex(
                name: "ix_user_families_user_id_is_current_family",
                schema: "auth",
                table: "user_families",
                columns: new[] { "user_id", "is_current_family" },
                filter: "is_current_family = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_families_user_id_is_current_family",
                schema: "auth",
                table: "user_families");

            migrationBuilder.DropColumn(
                name: "is_current_family",
                schema: "auth",
                table: "user_families");
        }
    }
}
