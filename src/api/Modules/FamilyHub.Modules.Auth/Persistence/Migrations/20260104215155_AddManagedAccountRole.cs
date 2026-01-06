using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Modules.Auth.Persistence.Migrations
{
    /// <summary>
    /// Phase 0.1 of CHILD → MANAGED_ACCOUNT migration (Epic #24).
    ///
    /// This migration adds the MANAGED_ACCOUNT role value to the C# UserRole value object
    /// while retaining CHILD for backward compatibility. No database schema changes are needed
    /// because UserRole is stored as a string column, not a PostgreSQL enum.
    ///
    /// Migration Strategy (3-Step Safe Migration):
    /// - Step 0.1 (THIS STEP): Add MANAGED_ACCOUNT to C# code, keep CHILD
    /// - Step 0.2: Migrate any existing CHILD data to MANAGED_ACCOUNT (data migration)
    /// - Step 0.3: Remove CHILD from C# code after verification
    ///
    /// Rollback: No database changes to rollback. C# code rollback via git revert.
    /// </summary>
    /// <inheritdoc />
    public partial class AddManagedAccountRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No database schema changes required.
            // UserRole is stored as VARCHAR and both "child" and "managed_account" are valid.
            // The C# UserRole value object now accepts both values via ValidRoles array.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No database schema changes to revert.
            // Rollback via git revert on C# code changes.
        }
    }
}
