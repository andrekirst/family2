using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameFullNameToPersonName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "full_name",
                schema: "auth",
                table: "users",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "full_name",
                schema: "auth",
                table: "queued_managed_account_creations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "full_name",
                schema: "auth",
                table: "family_member_invitations",
                newName: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                schema: "auth",
                table: "users",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "auth",
                table: "queued_managed_account_creations",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "auth",
                table: "family_member_invitations",
                newName: "full_name");
        }
    }
}
