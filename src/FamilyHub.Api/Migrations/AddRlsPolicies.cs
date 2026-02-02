using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing policies if they exist (idempotent migration)
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS user_self_policy ON auth.users;
                DROP POLICY IF EXISTS family_member_policy ON family.families;
            ");

            // Enable Row-Level Security on auth.users table
            migrationBuilder.Sql(@"
                ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
            ");

            // Create policy for users to see only their own user record
            // Uses session variable set by PostgresRlsMiddleware
            migrationBuilder.Sql(@"
                CREATE POLICY user_self_policy ON auth.users
                    FOR ALL
                    USING (""Id""::text = current_setting('app.current_user_id', true));
            ");

            // Enable Row-Level Security on family.families table
            migrationBuilder.Sql(@"
                ALTER TABLE family.families ENABLE ROW LEVEL SECURITY;
            ");

            // Create policy for users to see only their own family
            // Uses session variable set by PostgresRlsMiddleware
            migrationBuilder.Sql(@"
                CREATE POLICY family_member_policy ON family.families
                    FOR ALL
                    USING (""Id""::text = current_setting('app.current_family_id', true));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop RLS policies
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS user_self_policy ON auth.users;
                DROP POLICY IF EXISTS family_member_policy ON family.families;
            ");

            // Disable RLS
            migrationBuilder.Sql(@"
                ALTER TABLE auth.users DISABLE ROW LEVEL SECURITY;
                ALTER TABLE family.families DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
