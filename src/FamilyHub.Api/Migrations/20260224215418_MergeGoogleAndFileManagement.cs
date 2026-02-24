using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// No-op merge migration. Both schemas (file_management + google_integration) were already
    /// created by their respective branch migrations (AddFileManagementEntities + AddGoogleIntegration).
    /// This migration exists only to synchronize the EF Core model snapshot after merging
    /// the two branches. The snapshot now reflects both schemas.
    /// </remarks>
    public partial class MergeGoogleAndFileManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — tables already exist from branch-specific migrations.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — corresponding Up() is a no-op.
        }
    }
}
