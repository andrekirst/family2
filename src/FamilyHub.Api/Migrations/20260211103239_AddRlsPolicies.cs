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
            // Auth RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS user_self_policy ON auth.users;
                ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
                CREATE POLICY user_self_policy ON auth.users
                    FOR ALL
                    USING (""id""::text = current_setting('app.current_user_id', true));
            ");

            // Family RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS family_member_policy ON family.families;
                ALTER TABLE family.families ENABLE ROW LEVEL SECURITY;
                CREATE POLICY family_member_policy ON family.families
                    FOR ALL
                    USING (""id""::text = current_setting('app.current_family_id', true));
            ");

            // Calendar events RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS calendar_event_family_policy ON calendar.calendar_events;
                ALTER TABLE calendar.calendar_events ENABLE ROW LEVEL SECURITY;
                CREATE POLICY calendar_event_family_policy ON calendar.calendar_events
                    FOR ALL
                    USING (""family_id""::text = current_setting('app.current_family_id', true));
            ");

            // Calendar attendees RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS calendar_attendee_family_policy ON calendar.calendar_event_attendees;
                ALTER TABLE calendar.calendar_event_attendees ENABLE ROW LEVEL SECURITY;
                CREATE POLICY calendar_attendee_family_policy ON calendar.calendar_event_attendees
                    FOR ALL
                    USING (""calendar_event_id"" IN (
                        SELECT ""id"" FROM calendar.calendar_events
                        WHERE ""family_id""::text = current_setting('app.current_family_id', true)
                    ));
            ");

            // EventChain chain_definitions RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS chain_definition_family_policy ON event_chain.chain_definitions;
                ALTER TABLE event_chain.chain_definitions ENABLE ROW LEVEL SECURITY;
                CREATE POLICY chain_definition_family_policy ON event_chain.chain_definitions
                    FOR ALL
                    USING (""family_id""::text = current_setting('app.current_family_id', true));
            ");

            // EventChain chain_executions RLS
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS chain_execution_family_policy ON event_chain.chain_executions;
                ALTER TABLE event_chain.chain_executions ENABLE ROW LEVEL SECURITY;
                CREATE POLICY chain_execution_family_policy ON event_chain.chain_executions
                    FOR ALL
                    USING (""family_id""::text = current_setting('app.current_family_id', true));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS user_self_policy ON auth.users;
                DROP POLICY IF EXISTS family_member_policy ON family.families;
                DROP POLICY IF EXISTS calendar_event_family_policy ON calendar.calendar_events;
                DROP POLICY IF EXISTS calendar_attendee_family_policy ON calendar.calendar_event_attendees;
                DROP POLICY IF EXISTS chain_definition_family_policy ON event_chain.chain_definitions;
                DROP POLICY IF EXISTS chain_execution_family_policy ON event_chain.chain_executions;
                ALTER TABLE auth.users DISABLE ROW LEVEL SECURITY;
                ALTER TABLE family.families DISABLE ROW LEVEL SECURITY;
                ALTER TABLE calendar.calendar_events DISABLE ROW LEVEL SECURITY;
                ALTER TABLE calendar.calendar_event_attendees DISABLE ROW LEVEL SECURITY;
                ALTER TABLE event_chain.chain_definitions DISABLE ROW LEVEL SECURITY;
                ALTER TABLE event_chain.chain_executions DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
