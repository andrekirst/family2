using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIntegrationRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Google Account Links RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS google_account_link_user_policy ON google_integration.google_account_links;
                ALTER TABLE google_integration.google_account_links ENABLE ROW LEVEL SECURITY;
                CREATE POLICY google_account_link_user_policy ON google_integration.google_account_links
                    FOR ALL
                    USING (""user_id""::text = current_setting('app.current_user_id', true));
            ");

            // OAuth States RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS oauth_state_user_policy ON google_integration.oauth_states;
                ALTER TABLE google_integration.oauth_states ENABLE ROW LEVEL SECURITY;
                CREATE POLICY oauth_state_user_policy ON google_integration.oauth_states
                    FOR ALL
                    USING (""user_id""::text = current_setting('app.current_user_id', true));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS google_account_link_user_policy ON google_integration.google_account_links;
                DROP POLICY IF EXISTS oauth_state_user_policy ON google_integration.oauth_states;
                ALTER TABLE google_integration.google_account_links DISABLE ROW LEVEL SECURITY;
                ALTER TABLE google_integration.oauth_states DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
