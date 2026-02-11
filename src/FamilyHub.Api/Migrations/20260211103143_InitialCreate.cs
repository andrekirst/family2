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
                name: "calendar");

            migrationBuilder.EnsureSchema(
                name: "event_chain");

            migrationBuilder.EnsureSchema(
                name: "family");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "calendar_events",
                schema: "calendar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_all_day = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chain_definitions",
                schema: "event_chain",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_template = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    template_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    trigger_event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    trigger_module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trigger_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    trigger_output_schema = table.Column<string>(type: "jsonb", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chain_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chain_entity_mappings",
                schema: "event_chain",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chain_entity_mappings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_event_attendees",
                schema: "calendar",
                columns: table => new
                {
                    calendar_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_event_attendees", x => new { x.calendar_event_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_calendar_event_attendees_calendar_events_calendar_event_id",
                        column: x => x.calendar_event_id,
                        principalSchema: "calendar",
                        principalTable: "calendar_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chain_definition_steps",
                schema: "event_chain",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    action_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    action_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    input_mappings = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    condition_expression = table.Column<string>(type: "text", nullable: true),
                    is_compensatable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    compensation_action_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    step_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chain_definition_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_chain_definition_steps_chain_definitions_chain_definition_id",
                        column: x => x.chain_definition_id,
                        principalSchema: "event_chain",
                        principalTable: "chain_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chain_executions",
                schema: "event_chain",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    trigger_event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    trigger_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger_payload = table.Column<string>(type: "jsonb", nullable: false),
                    context = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    current_step_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chain_executions", x => x.id);
                    table.ForeignKey(
                        name: "fk_chain_executions_chain_definitions_chain_definition_id",
                        column: x => x.chain_definition_id,
                        principalSchema: "event_chain",
                        principalTable: "chain_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "step_executions",
                schema: "event_chain",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    step_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    action_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    input_payload = table.Column<string>(type: "jsonb", nullable: true),
                    output_payload = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_retries = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    step_order = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    compensated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_step_executions", x => x.id);
                    table.ForeignKey(
                        name: "fk_step_executions_chain_executions_chain_execution_id",
                        column: x => x.chain_execution_id,
                        principalSchema: "event_chain",
                        principalTable: "chain_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chain_scheduled_jobs",
                schema: "event_chain",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chain_scheduled_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_chain_scheduled_jobs_chain_executions_chain_execution_id",
                        column: x => x.chain_execution_id,
                        principalSchema: "event_chain",
                        principalTable: "chain_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_chain_scheduled_jobs_step_executions_step_execution_id",
                        column: x => x.step_execution_id,
                        principalSchema: "event_chain",
                        principalTable: "step_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "families",
                schema: "family",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    external_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "KEYCLOAK"),
                    family_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "family_invitations",
                schema: "family",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invitee_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_family_invitations_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_family_invitations_users_accepted_by_user_id",
                        column: x => x.accepted_by_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_family_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "family_members",
                schema: "family",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_family_members_families_family_id",
                        column: x => x.family_id,
                        principalSchema: "family",
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_family_members_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_events_created_by_start_time",
                schema: "calendar",
                table: "calendar_events",
                columns: new[] { "created_by", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_events_family_id_start_time",
                schema: "calendar",
                table: "calendar_events",
                columns: new[] { "family_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_chain_definition_steps_definition_id",
                schema: "event_chain",
                table: "chain_definition_steps",
                column: "chain_definition_id");

            migrationBuilder.CreateIndex(
                name: "uq_chain_step_alias",
                schema: "event_chain",
                table: "chain_definition_steps",
                columns: new[] { "chain_definition_id", "alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_chain_step_order",
                schema: "event_chain",
                table: "chain_definition_steps",
                columns: new[] { "chain_definition_id", "step_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chain_definitions_family_id",
                schema: "event_chain",
                table: "chain_definitions",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_chain_definitions_template_name",
                schema: "event_chain",
                table: "chain_definitions",
                column: "template_name",
                filter: "is_template = true");

            migrationBuilder.CreateIndex(
                name: "ix_chain_definitions_trigger_event_type",
                schema: "event_chain",
                table: "chain_definitions",
                column: "trigger_event_type",
                filter: "is_enabled = true");

            migrationBuilder.CreateIndex(
                name: "ix_chain_entity_mappings_entity",
                schema: "event_chain",
                table: "chain_entity_mappings",
                columns: new[] { "entity_id", "entity_type" });

            migrationBuilder.CreateIndex(
                name: "ix_chain_entity_mappings_execution_id",
                schema: "event_chain",
                table: "chain_entity_mappings",
                column: "chain_execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_chain_entity_mappings_module",
                schema: "event_chain",
                table: "chain_entity_mappings",
                columns: new[] { "module", "entity_type" });

            migrationBuilder.CreateIndex(
                name: "ix_chain_executions_correlation_id",
                schema: "event_chain",
                table: "chain_executions",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_chain_executions_definition_id",
                schema: "event_chain",
                table: "chain_executions",
                column: "chain_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_chain_executions_family_id",
                schema: "event_chain",
                table: "chain_executions",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_chain_executions_status",
                schema: "event_chain",
                table: "chain_executions",
                column: "status",
                filter: "status IN ('Pending', 'Running', 'Compensating')");

            migrationBuilder.CreateIndex(
                name: "ix_chain_scheduled_jobs_chain_execution_id",
                schema: "event_chain",
                table: "chain_scheduled_jobs",
                column: "chain_execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_chain_scheduled_jobs_ready",
                schema: "event_chain",
                table: "chain_scheduled_jobs",
                column: "scheduled_at",
                filter: "picked_up_at IS NULL AND completed_at IS NULL AND failed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_chain_scheduled_jobs_stale",
                schema: "event_chain",
                table: "chain_scheduled_jobs",
                column: "picked_up_at",
                filter: "completed_at IS NULL AND failed_at IS NULL AND picked_up_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_chain_scheduled_jobs_step_execution_id",
                schema: "event_chain",
                table: "chain_scheduled_jobs",
                column: "step_execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_families_owner_id",
                schema: "family",
                table: "families",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_invitations_accepted_by_user_id",
                schema: "family",
                table: "family_invitations",
                column: "accepted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_invitations_family_id_status",
                schema: "family",
                table: "family_invitations",
                columns: new[] { "family_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_family_invitations_invited_by_user_id",
                schema: "family",
                table: "family_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_invitations_token_hash",
                schema: "family",
                table: "family_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_members_family_id_user_id",
                schema: "family",
                table: "family_members",
                columns: new[] { "family_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_members_user_id",
                schema: "family",
                table: "family_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_step_executions_chain_execution_id",
                schema: "event_chain",
                table: "step_executions",
                column: "chain_execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_step_executions_scheduled",
                schema: "event_chain",
                table: "step_executions",
                column: "scheduled_at",
                filter: "status = 'Pending' AND scheduled_at IS NOT NULL AND picked_up_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_step_executions_status",
                schema: "event_chain",
                table: "step_executions",
                column: "status",
                filter: "status IN ('Pending', 'Running')");

            migrationBuilder.CreateIndex(
                name: "uq_step_execution_alias",
                schema: "event_chain",
                table: "step_executions",
                columns: new[] { "chain_execution_id", "step_alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_external_user_id",
                schema: "auth",
                table: "users",
                column: "external_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_family_id",
                schema: "auth",
                table: "users",
                column: "family_id");

            migrationBuilder.AddForeignKey(
                name: "fk_families_users_owner_id",
                schema: "family",
                table: "families",
                column: "owner_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_families_users_owner_id",
                schema: "family",
                table: "families");

            migrationBuilder.DropTable(
                name: "calendar_event_attendees",
                schema: "calendar");

            migrationBuilder.DropTable(
                name: "chain_definition_steps",
                schema: "event_chain");

            migrationBuilder.DropTable(
                name: "chain_entity_mappings",
                schema: "event_chain");

            migrationBuilder.DropTable(
                name: "chain_scheduled_jobs",
                schema: "event_chain");

            migrationBuilder.DropTable(
                name: "family_invitations",
                schema: "family");

            migrationBuilder.DropTable(
                name: "family_members",
                schema: "family");

            migrationBuilder.DropTable(
                name: "calendar_events",
                schema: "calendar");

            migrationBuilder.DropTable(
                name: "step_executions",
                schema: "event_chain");

            migrationBuilder.DropTable(
                name: "chain_executions",
                schema: "event_chain");

            migrationBuilder.DropTable(
                name: "chain_definitions",
                schema: "event_chain");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "families",
                schema: "family");
        }
    }
}
