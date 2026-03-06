-- Event Chain module: schema, chain_definitions, chain_definition_steps,
-- chain_executions, step_executions, chain_scheduled_jobs, chain_entity_mappings
CREATE SCHEMA IF NOT EXISTS event_chain;

CREATE TABLE IF NOT EXISTS event_chain.chain_definitions (
    id uuid NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    family_id uuid NOT NULL,
    created_by_user_id uuid NOT NULL,
    is_enabled boolean NOT NULL DEFAULT true,
    is_template boolean NOT NULL DEFAULT false,
    template_name character varying(100),
    trigger_event_type character varying(500) NOT NULL,
    trigger_module character varying(100) NOT NULL,
    trigger_description character varying(500),
    trigger_output_schema jsonb,
    version integer NOT NULL DEFAULT 1,
    created_at timestamp with time zone NOT NULL DEFAULT now(),
    updated_at timestamp with time zone NOT NULL DEFAULT now(),
    CONSTRAINT pk_chain_definitions PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_chain_definitions_family_id ON event_chain.chain_definitions (family_id);
CREATE INDEX IF NOT EXISTS ix_chain_definitions_template_name ON event_chain.chain_definitions (template_name) WHERE is_template = true;
CREATE INDEX IF NOT EXISTS ix_chain_definitions_trigger_event_type ON event_chain.chain_definitions (trigger_event_type) WHERE is_enabled = true;

CREATE TABLE IF NOT EXISTS event_chain.chain_definition_steps (
    id uuid NOT NULL,
    chain_definition_id uuid NOT NULL,
    alias character varying(50) NOT NULL,
    name character varying(200) NOT NULL,
    action_type character varying(500) NOT NULL,
    action_version character varying(20) NOT NULL,
    module character varying(100) NOT NULL,
    input_mappings jsonb NOT NULL DEFAULT '{}',
    condition_expression text,
    is_compensatable boolean NOT NULL DEFAULT false,
    compensation_action_type character varying(500),
    step_order integer NOT NULL,
    CONSTRAINT pk_chain_definition_steps PRIMARY KEY (id),
    CONSTRAINT fk_chain_definition_steps_chain_definitions_chain_definition_id FOREIGN KEY (chain_definition_id)
        REFERENCES event_chain.chain_definitions (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_chain_definition_steps_definition_id ON event_chain.chain_definition_steps (chain_definition_id);
CREATE UNIQUE INDEX IF NOT EXISTS uq_chain_step_alias ON event_chain.chain_definition_steps (chain_definition_id, alias);
CREATE UNIQUE INDEX IF NOT EXISTS uq_chain_step_order ON event_chain.chain_definition_steps (chain_definition_id, step_order);

CREATE TABLE IF NOT EXISTS event_chain.chain_executions (
    id uuid NOT NULL,
    chain_definition_id uuid NOT NULL,
    family_id uuid NOT NULL,
    correlation_id uuid NOT NULL DEFAULT gen_random_uuid(),
    status character varying(30) NOT NULL DEFAULT 'Pending',
    trigger_event_type character varying(500) NOT NULL,
    trigger_event_id uuid NOT NULL,
    trigger_payload jsonb NOT NULL,
    context jsonb NOT NULL DEFAULT '{}',
    current_step_index integer NOT NULL DEFAULT 0,
    started_at timestamp with time zone NOT NULL DEFAULT now(),
    completed_at timestamp with time zone,
    failed_at timestamp with time zone,
    error_message text,
    CONSTRAINT pk_chain_executions PRIMARY KEY (id),
    CONSTRAINT fk_chain_executions_chain_definitions_chain_definition_id FOREIGN KEY (chain_definition_id)
        REFERENCES event_chain.chain_definitions (id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_chain_executions_correlation_id ON event_chain.chain_executions (correlation_id);
CREATE INDEX IF NOT EXISTS ix_chain_executions_definition_id ON event_chain.chain_executions (chain_definition_id);
CREATE INDEX IF NOT EXISTS ix_chain_executions_family_id ON event_chain.chain_executions (family_id);
CREATE INDEX IF NOT EXISTS ix_chain_executions_status ON event_chain.chain_executions (status) WHERE status IN ('Pending', 'Running', 'Compensating');

CREATE TABLE IF NOT EXISTS event_chain.step_executions (
    id uuid NOT NULL,
    chain_execution_id uuid NOT NULL,
    step_alias character varying(50) NOT NULL,
    step_name character varying(200) NOT NULL,
    action_type character varying(500) NOT NULL,
    status character varying(30) NOT NULL DEFAULT 'Pending',
    input_payload jsonb,
    output_payload jsonb,
    error_message text,
    retry_count integer NOT NULL DEFAULT 0,
    max_retries integer NOT NULL DEFAULT 3,
    step_order integer NOT NULL,
    scheduled_at timestamp with time zone,
    picked_up_at timestamp with time zone,
    started_at timestamp with time zone,
    completed_at timestamp with time zone,
    compensated_at timestamp with time zone,
    CONSTRAINT pk_step_executions PRIMARY KEY (id),
    CONSTRAINT fk_step_executions_chain_executions_chain_execution_id FOREIGN KEY (chain_execution_id)
        REFERENCES event_chain.chain_executions (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_step_executions_chain_execution_id ON event_chain.step_executions (chain_execution_id);
CREATE INDEX IF NOT EXISTS ix_step_executions_scheduled ON event_chain.step_executions (scheduled_at) WHERE status = 'Pending' AND scheduled_at IS NOT NULL AND picked_up_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_step_executions_status ON event_chain.step_executions (status) WHERE status IN ('Pending', 'Running');
CREATE UNIQUE INDEX IF NOT EXISTS uq_step_execution_alias ON event_chain.step_executions (chain_execution_id, step_alias);

CREATE TABLE IF NOT EXISTS event_chain.chain_scheduled_jobs (
    id uuid NOT NULL,
    step_execution_id uuid NOT NULL,
    chain_execution_id uuid NOT NULL,
    scheduled_at timestamp with time zone NOT NULL,
    picked_up_at timestamp with time zone,
    completed_at timestamp with time zone,
    failed_at timestamp with time zone,
    error_message text,
    retry_count integer NOT NULL DEFAULT 0,
    created_at timestamp with time zone NOT NULL DEFAULT now(),
    CONSTRAINT pk_chain_scheduled_jobs PRIMARY KEY (id),
    CONSTRAINT fk_chain_scheduled_jobs_chain_executions_chain_execution_id FOREIGN KEY (chain_execution_id)
        REFERENCES event_chain.chain_executions (id) ON DELETE CASCADE,
    CONSTRAINT fk_chain_scheduled_jobs_step_executions_step_execution_id FOREIGN KEY (step_execution_id)
        REFERENCES event_chain.step_executions (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_chain_scheduled_jobs_chain_execution_id ON event_chain.chain_scheduled_jobs (chain_execution_id);
CREATE INDEX IF NOT EXISTS ix_chain_scheduled_jobs_ready ON event_chain.chain_scheduled_jobs (scheduled_at) WHERE picked_up_at IS NULL AND completed_at IS NULL AND failed_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_chain_scheduled_jobs_stale ON event_chain.chain_scheduled_jobs (picked_up_at) WHERE completed_at IS NULL AND failed_at IS NULL AND picked_up_at IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_chain_scheduled_jobs_step_execution_id ON event_chain.chain_scheduled_jobs (step_execution_id);

CREATE TABLE IF NOT EXISTS event_chain.chain_entity_mappings (
    id uuid NOT NULL,
    chain_execution_id uuid NOT NULL,
    step_alias character varying(50) NOT NULL,
    entity_type character varying(200) NOT NULL,
    entity_id uuid NOT NULL,
    module character varying(100) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT now(),
    CONSTRAINT pk_chain_entity_mappings PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_chain_entity_mappings_entity ON event_chain.chain_entity_mappings (entity_id, entity_type);
CREATE INDEX IF NOT EXISTS ix_chain_entity_mappings_execution_id ON event_chain.chain_entity_mappings (chain_execution_id);
CREATE INDEX IF NOT EXISTS ix_chain_entity_mappings_module ON event_chain.chain_entity_mappings (module, entity_type);
