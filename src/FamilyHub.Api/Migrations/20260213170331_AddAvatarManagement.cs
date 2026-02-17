using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "avatar");

            migrationBuilder.EnsureSchema(
                name: "storage");

            migrationBuilder.AddColumn<Guid>(
                name: "avatar_id",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "avatar_id",
                schema: "family",
                table: "family_members",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "avatars",
                schema: "avatar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    original_mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avatars", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stored_files",
                schema: "storage",
                columns: table => new
                {
                    storage_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stored_files", x => x.storage_key);
                });

            migrationBuilder.CreateTable(
                name: "avatar_variants",
                schema: "avatar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avatar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_size = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avatar_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_avatar_variants_avatars_avatar_id",
                        column: x => x.avatar_id,
                        principalSchema: "avatar",
                        principalTable: "avatars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_avatar_variants_avatar_id_size",
                schema: "avatar",
                table: "avatar_variants",
                columns: new[] { "avatar_id", "size" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avatar_variants",
                schema: "avatar");

            migrationBuilder.DropTable(
                name: "stored_files",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "avatars",
                schema: "avatar");

            migrationBuilder.DropColumn(
                name: "avatar_id",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_id",
                schema: "family",
                table: "family_members");
        }
    }
}
