using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInvitedByFromUserFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invited_by",
                schema: "auth",
                table: "user_families");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "invited_by",
                schema: "auth",
                table: "user_families",
                type: "uuid",
                nullable: true);
        }
    }
}
