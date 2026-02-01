using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "family");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "families",
                schema: "family",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExternalProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "KEYCLOAK"),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_families_OwnerId",
                schema: "family",
                table: "families",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "auth",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ExternalUserId",
                schema: "auth",
                table: "users",
                column: "ExternalUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_FamilyId",
                schema: "auth",
                table: "users",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_families_users_OwnerId",
                schema: "family",
                table: "families",
                column: "OwnerId",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Enable Row-Level Security (RLS) policies for multi-tenant data isolation
            migrationBuilder.Sql(@"
                -- Enable RLS on users table
                ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

                -- Policy: Users can see their own record
                CREATE POLICY user_self_policy ON auth.users
                    USING (""Id""::text = current_setting('app.current_user_id', true));

                -- Policy: Users can see members of their family
                CREATE POLICY user_family_policy ON auth.users
                    USING (
                        ""FamilyId"" IS NOT NULL
                        AND ""FamilyId""::text = current_setting('app.current_family_id', true)
                    );

                -- Enable RLS on families table
                ALTER TABLE family.families ENABLE ROW LEVEL SECURITY;

                -- Policy: Family owner can see their family
                CREATE POLICY family_owner_policy ON family.families
                    USING (""OwnerId""::text = current_setting('app.current_user_id', true));

                -- Policy: Family members can see their family
                CREATE POLICY family_member_policy ON family.families
                    USING (""Id""::text = current_setting('app.current_family_id', true));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop RLS policies before dropping tables
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS user_self_policy ON auth.users;
                DROP POLICY IF EXISTS user_family_policy ON auth.users;
                DROP POLICY IF EXISTS family_owner_policy ON family.families;
                DROP POLICY IF EXISTS family_member_policy ON family.families;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_families_users_OwnerId",
                schema: "family",
                table: "families");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "families",
                schema: "family");
        }
    }
}
