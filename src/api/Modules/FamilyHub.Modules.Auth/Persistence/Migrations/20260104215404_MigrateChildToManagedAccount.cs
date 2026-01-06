using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <summary>
    /// Phase 0.2 of CHILD → MANAGED_ACCOUNT migration (Epic #24).
    ///
    /// This migration would migrate any existing 'child' role data to 'managed_account'.
    /// However, investigation reveals that UserRole is NOT persisted in the database.
    /// Roles are computed dynamically via User.GetRoleInFamily() method based on ownership.
    ///
    /// Current Implementation:
    /// - No 'role' column in auth.users table
    /// - Roles determined by: user.Id == family.OwnerId ? Owner : Member
    /// - UserRole enum exists in C# but is not stored in database
    ///
    /// Therefore, this migration is a NO-OP with verification queries only.
    ///
    /// Future Consideration:
    /// - When invitation system is implemented (Phase 1), roles may be persisted
    /// - At that time, a real data migration would be needed
    ///
    /// Verification: Confirms no 'role' column exists and no CHILD data to migrate.
    /// </summary>
    /// <inheritdoc />
    public partial class MigrateChildToManagedAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verification Query 1: Confirm no 'role' column exists in users table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'auth'
                          AND table_name = 'users'
                          AND column_name = 'role'
                    ) THEN
                        RAISE EXCEPTION 'Unexpected: role column exists in auth.users table. Migration assumptions violated.';
                    ELSE
                        RAISE NOTICE 'Verification passed: No role column in auth.users table (roles are computed dynamically).';
                    END IF;
                END $$;
            ");

            // Verification Query 2: Log current user count for audit trail
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    user_count INTEGER;
                BEGIN
                    SELECT COUNT(*) INTO user_count FROM auth.users WHERE deleted_at IS NULL;
                    RAISE NOTICE 'Current active user count: %', user_count;
                    RAISE NOTICE 'Migration Phase 0.2: No data migration needed (UserRole not persisted in database).';
                END $$;
            ");

            // NO actual data migration - UserRole is not stored in database
            // This migration serves as documentation and verification checkpoint
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No data changes to rollback
            // This migration was verification-only
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    RAISE NOTICE 'Rollback Phase 0.2: No data rollback needed (migration was verification-only).';
                END $$;
            ");
        }
    }
}
