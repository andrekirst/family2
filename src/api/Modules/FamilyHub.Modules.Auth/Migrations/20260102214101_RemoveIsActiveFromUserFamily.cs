using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsActiveFromUserFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_families_is_active",
                schema: "auth",
                table: "user_families");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "auth",
                table: "user_families");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "auth",
                table: "user_families",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_families_is_active",
                schema: "auth",
                table: "user_families",
                column: "is_active");
        }
    }
}
