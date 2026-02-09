using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyMembersAndInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create family_members table
            migrationBuilder.CreateTable(
                name: "family_members",
                schema: "family",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_family_members_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_family_members_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_family_members_FamilyId_UserId",
                schema: "family",
                table: "family_members",
                columns: new[] { "FamilyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_family_members_UserId",
                schema: "family",
                table: "family_members",
                column: "UserId");

            // Create family_invitations table
            migrationBuilder.CreateTable(
                name: "family_invitations",
                schema: "family",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_family_invitations_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_family_invitations_users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_invitations_users_AcceptedByUserId",
                        column: x => x.AcceptedByUserId,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_TokenHash",
                schema: "family",
                table: "family_invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_FamilyId_Status",
                schema: "family",
                table: "family_invitations",
                columns: new[] { "FamilyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_InvitedByUserId",
                schema: "family",
                table: "family_invitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_AcceptedByUserId",
                schema: "family",
                table: "family_invitations",
                column: "AcceptedByUserId");

            // RLS policies for family_members
            migrationBuilder.Sql(@"
                ALTER TABLE family.family_members ENABLE ROW LEVEL SECURITY;

                -- Members can see other members of their family
                CREATE POLICY family_members_read_policy ON family.family_members
                    FOR SELECT
                    USING (""FamilyId""::text = current_setting('app.current_family_id', true));

                -- Members can only be inserted/updated by family admins (enforced at application level)
                CREATE POLICY family_members_write_policy ON family.family_members
                    FOR ALL
                    USING (""FamilyId""::text = current_setting('app.current_family_id', true));
            ");

            // RLS policies for family_invitations
            migrationBuilder.Sql(@"
                ALTER TABLE family.family_invitations ENABLE ROW LEVEL SECURITY;

                -- Family members can see invitations for their family
                CREATE POLICY family_invitations_read_policy ON family.family_invitations
                    FOR SELECT
                    USING (""FamilyId""::text = current_setting('app.current_family_id', true));

                -- Invitations can only be created/modified by family members (enforced at application level)
                CREATE POLICY family_invitations_write_policy ON family.family_invitations
                    FOR ALL
                    USING (""FamilyId""::text = current_setting('app.current_family_id', true));
            ");

            // Data migration: Create FamilyMember records for existing family owners
            migrationBuilder.Sql(@"
                INSERT INTO family.family_members (""Id"", ""FamilyId"", ""UserId"", ""Role"", ""JoinedAt"", ""IsActive"")
                SELECT
                    gen_random_uuid(),
                    f.""Id"",
                    f.""OwnerId"",
                    'Owner',
                    f.""CreatedAt"",
                    true
                FROM family.families f
                WHERE NOT EXISTS (
                    SELECT 1 FROM family.family_members fm
                    WHERE fm.""FamilyId"" = f.""Id"" AND fm.""UserId"" = f.""OwnerId""
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop RLS policies
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS family_members_read_policy ON family.family_members;
                DROP POLICY IF EXISTS family_members_write_policy ON family.family_members;
                DROP POLICY IF EXISTS family_invitations_read_policy ON family.family_invitations;
                DROP POLICY IF EXISTS family_invitations_write_policy ON family.family_invitations;
            ");

            migrationBuilder.DropTable(
                name: "family_invitations",
                schema: "family");

            migrationBuilder.DropTable(
                name: "family_members",
                schema: "family");
        }
    }
}
