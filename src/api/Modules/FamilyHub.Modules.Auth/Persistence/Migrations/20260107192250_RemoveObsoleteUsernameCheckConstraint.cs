using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveObsoleteUsernameCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only drop the constraint if it exists (for existing databases)
            // New databases won't have this constraint since it was removed from the model
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'ck_family_member_invitations_email_xor_username'
                        AND connamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'auth')
                    ) THEN
                        ALTER TABLE auth.family_member_invitations
                        DROP CONSTRAINT ck_family_member_invitations_email_xor_username;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_family_member_invitations_email_xor_username",
                schema: "auth",
                table: "family_member_invitations",
                sql: "(email IS NOT NULL AND username IS NULL) OR (email IS NULL AND username IS NOT NULL)");
        }
    }
}
