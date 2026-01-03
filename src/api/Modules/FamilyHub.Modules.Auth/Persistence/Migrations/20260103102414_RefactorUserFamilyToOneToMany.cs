using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUserFamilyToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_families",
                schema: "auth");

            migrationBuilder.AddColumn<Guid>(
                name: "family_id",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_users_family_id",
                schema: "auth",
                table: "users",
                column: "family_id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_families_family_id",
                schema: "auth",
                table: "users",
                column: "family_id",
                principalSchema: "auth",
                principalTable: "families",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_families_family_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_family_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "family_id",
                schema: "auth",
                table: "users");

            migrationBuilder.CreateTable(
                name: "user_families",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_current_family = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_families", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_families_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "auth",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_families_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_families_family_id",
                schema: "auth",
                table: "user_families",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_families_user_family",
                schema: "auth",
                table: "user_families",
                columns: new[] { "user_id", "family_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_families_user_id_is_current_family",
                schema: "auth",
                table: "user_families",
                columns: new[] { "user_id", "is_current_family" },
                filter: "is_current_family = true");
        }
    }
}
