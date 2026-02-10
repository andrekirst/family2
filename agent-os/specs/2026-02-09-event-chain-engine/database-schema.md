# Event Chain Engine — PostgreSQL Database Schema

Complete approved database schema for the Event Chain Engine. All tables in the dedicated `event_chain` schema with RLS policies and indexes.

---

## Schema Overview

```
event_chain schema (6 tables)
├── chain_definitions        — Chain templates/blueprints
├── chain_definition_steps   — Step definitions within a chain
├── chain_executions         — Runtime saga instances
├── step_executions          — Individual step results
├── chain_entity_mappings    — Entities created by chains
└── chain_scheduled_jobs     — Delayed/scheduled step queue
```

---

## Table 1: chain_definitions

Chain templates that define the blueprint for automated workflows.

```sql
CREATE TABLE event_chain.chain_definitions (
    id                  UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    family_id           UUID            NOT NULL,
    name                VARCHAR(200)    NOT NULL,
    description         TEXT,
    is_enabled          BOOLEAN         NOT NULL DEFAULT true,
    is_template         BOOLEAN         NOT NULL DEFAULT false,
    template_name       VARCHAR(100),
    trigger_event_type  VARCHAR(500)    NOT NULL,
    trigger_module      VARCHAR(100)    NOT NULL,
    trigger_description VARCHAR(500),
    trigger_output_schema JSONB,
    created_by_user_id  UUID            NOT NULL,
    version             INTEGER         NOT NULL DEFAULT 1,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- Indexes
CREATE INDEX ix_chain_definitions_family_id
    ON event_chain.chain_definitions (family_id);

CREATE INDEX ix_chain_definitions_trigger_event_type
    ON event_chain.chain_definitions (trigger_event_type)
    WHERE is_enabled = true;

CREATE INDEX ix_chain_definitions_template_name
    ON event_chain.chain_definitions (template_name)
    WHERE is_template = true;

-- RLS
ALTER TABLE event_chain.chain_definitions ENABLE ROW LEVEL SECURITY;

CREATE POLICY chain_definitions_family_policy
    ON event_chain.chain_definitions
    FOR ALL
    USING (family_id::text = current_setting('app.current_family_id', true));
```

---

## Table 2: chain_definition_steps

Ordered steps within a chain definition. Stored separately for normalization and easier querying.

```sql
CREATE TABLE event_chain.chain_definition_steps (
    id                          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    chain_definition_id         UUID            NOT NULL REFERENCES event_chain.chain_definitions(id) ON DELETE CASCADE,
    alias                       VARCHAR(50)     NOT NULL,
    name                        VARCHAR(200)    NOT NULL,
    action_type                 VARCHAR(500)    NOT NULL,
    action_version              VARCHAR(20)     NOT NULL,
    module                      VARCHAR(100)    NOT NULL,
    input_mappings              JSONB           NOT NULL DEFAULT '{}',
    condition_expression        TEXT,
    is_compensatable            BOOLEAN         NOT NULL DEFAULT false,
    compensation_action_type    VARCHAR(500),
    step_order                  INTEGER         NOT NULL,

    CONSTRAINT uq_chain_step_alias UNIQUE (chain_definition_id, alias),
    CONSTRAINT uq_chain_step_order UNIQUE (chain_definition_id, step_order)
);

-- Indexes
CREATE INDEX ix_chain_definition_steps_definition_id
    ON event_chain.chain_definition_steps (chain_definition_id);

-- RLS (cascades from chain_definitions via join)
ALTER TABLE event_chain.chain_definition_steps ENABLE ROW LEVEL SECURITY;

CREATE POLICY chain_definition_steps_family_policy
    ON event_chain.chain_definition_steps
    FOR ALL
    USING (chain_definition_id IN (
        SELECT id FROM event_chain.chain_definitions
        WHERE family_id::text = current_setting('app.current_family_id', true)
    ));
```

---

## Table 3: chain_executions

Runtime saga instances. One row per chain execution (when a trigger fires and a matching chain is found).

```sql
CREATE TABLE event_chain.chain_executions (
    id                      UUID                PRIMARY KEY DEFAULT gen_random_uuid(),
    chain_definition_id     UUID                NOT NULL REFERENCES event_chain.chain_definitions(id),
    family_id               UUID                NOT NULL,
    correlation_id          UUID                NOT NULL DEFAULT gen_random_uuid(),
    status                  VARCHAR(30)         NOT NULL DEFAULT 'Pending',
    trigger_event_type      VARCHAR(500)        NOT NULL,
    trigger_event_id        UUID                NOT NULL,
    trigger_payload         JSONB               NOT NULL,
    context                 JSONB               NOT NULL DEFAULT '{}',
    current_step_index      INTEGER             NOT NULL DEFAULT 0,
    started_at              TIMESTAMPTZ         NOT NULL DEFAULT now(),
    completed_at            TIMESTAMPTZ,
    failed_at               TIMESTAMPTZ,
    error_message           TEXT,

    CONSTRAINT chk_execution_status CHECK (
        status IN ('Pending', 'Running', 'Completed', 'PartiallyCompleted', 'Failed', 'Compensating', 'Compensated')
    )
);

-- Indexes
CREATE INDEX ix_chain_executions_family_id
    ON event_chain.chain_executions (family_id);

CREATE INDEX ix_chain_executions_definition_id
    ON event_chain.chain_executions (chain_definition_id);

CREATE INDEX ix_chain_executions_correlation_id
    ON event_chain.chain_executions (correlation_id);

CREATE INDEX ix_chain_executions_status
    ON event_chain.chain_executions (status)
    WHERE status IN ('Pending', 'Running', 'Compensating');

-- RLS
ALTER TABLE event_chain.chain_executions ENABLE ROW LEVEL SECURITY;

CREATE POLICY chain_executions_family_policy
    ON event_chain.chain_executions
    FOR ALL
    USING (family_id::text = current_setting('app.current_family_id', true));
```

---

## Table 4: step_executions

Individual step results within a chain execution. Tracks status, timing, payloads, and retry count.

```sql
CREATE TABLE event_chain.step_executions (
    id                      UUID                PRIMARY KEY DEFAULT gen_random_uuid(),
    chain_execution_id      UUID                NOT NULL REFERENCES event_chain.chain_executions(id) ON DELETE CASCADE,
    step_alias              VARCHAR(50)         NOT NULL,
    step_name               VARCHAR(200)        NOT NULL,
    action_type             VARCHAR(500)        NOT NULL,
    status                  VARCHAR(30)         NOT NULL DEFAULT 'Pending',
    input_payload           JSONB,
    output_payload          JSONB,
    error_message           TEXT,
    retry_count             INTEGER             NOT NULL DEFAULT 0,
    max_retries             INTEGER             NOT NULL DEFAULT 3,
    step_order              INTEGER             NOT NULL,
    scheduled_at            TIMESTAMPTZ,
    picked_up_at            TIMESTAMPTZ,
    started_at              TIMESTAMPTZ,
    completed_at            TIMESTAMPTZ,
    compensated_at          TIMESTAMPTZ,

    CONSTRAINT chk_step_status CHECK (
        status IN ('Pending', 'Running', 'Completed', 'Failed', 'Skipped', 'Compensating', 'Compensated')
    ),
    CONSTRAINT uq_step_execution_alias UNIQUE (chain_execution_id, step_alias)
);

-- Indexes
CREATE INDEX ix_step_executions_chain_execution_id
    ON event_chain.step_executions (chain_execution_id);

CREATE INDEX ix_step_executions_scheduled
    ON event_chain.step_executions (scheduled_at)
    WHERE status = 'Pending' AND scheduled_at IS NOT NULL AND picked_up_at IS NULL;

CREATE INDEX ix_step_executions_status
    ON event_chain.step_executions (status)
    WHERE status IN ('Pending', 'Running');

-- RLS (cascades from chain_executions via join)
ALTER TABLE event_chain.step_executions ENABLE ROW LEVEL SECURITY;

CREATE POLICY step_executions_family_policy
    ON event_chain.step_executions
    FOR ALL
    USING (chain_execution_id IN (
        SELECT id FROM event_chain.chain_executions
        WHERE family_id::text = current_setting('app.current_family_id', true)
    ));
```

---

## Table 5: chain_entity_mappings

Tracks entities created by chain steps for correlation and traceability.

```sql
CREATE TABLE event_chain.chain_entity_mappings (
    id                      UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    chain_execution_id      UUID            NOT NULL REFERENCES event_chain.chain_executions(id) ON DELETE CASCADE,
    step_alias              VARCHAR(50)     NOT NULL,
    entity_type             VARCHAR(200)    NOT NULL,
    entity_id               UUID            NOT NULL,
    module                  VARCHAR(100)    NOT NULL,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- Indexes
CREATE INDEX ix_chain_entity_mappings_execution_id
    ON event_chain.chain_entity_mappings (chain_execution_id);

CREATE INDEX ix_chain_entity_mappings_entity
    ON event_chain.chain_entity_mappings (entity_id, entity_type);

CREATE INDEX ix_chain_entity_mappings_module
    ON event_chain.chain_entity_mappings (module, entity_type);

-- RLS (cascades from chain_executions via join)
ALTER TABLE event_chain.chain_entity_mappings ENABLE ROW LEVEL SECURITY;

CREATE POLICY chain_entity_mappings_family_policy
    ON event_chain.chain_entity_mappings
    FOR ALL
    USING (chain_execution_id IN (
        SELECT id FROM event_chain.chain_executions
        WHERE family_id::text = current_setting('app.current_family_id', true)
    ));
```

---

## Table 6: chain_scheduled_jobs

Queue for delayed/scheduled step executions. Polled by the scheduler using `SELECT FOR UPDATE SKIP LOCKED`.

```sql
CREATE TABLE event_chain.chain_scheduled_jobs (
    id                      UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    step_execution_id       UUID            NOT NULL REFERENCES event_chain.step_executions(id) ON DELETE CASCADE,
    chain_execution_id      UUID            NOT NULL REFERENCES event_chain.chain_executions(id) ON DELETE CASCADE,
    scheduled_at            TIMESTAMPTZ     NOT NULL,
    picked_up_at            TIMESTAMPTZ,
    completed_at            TIMESTAMPTZ,
    failed_at               TIMESTAMPTZ,
    error_message           TEXT,
    retry_count             INTEGER         NOT NULL DEFAULT 0,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- Indexes (critical for scheduler performance)
CREATE INDEX ix_chain_scheduled_jobs_ready
    ON event_chain.chain_scheduled_jobs (scheduled_at)
    WHERE picked_up_at IS NULL AND completed_at IS NULL AND failed_at IS NULL;

CREATE INDEX ix_chain_scheduled_jobs_stale
    ON event_chain.chain_scheduled_jobs (picked_up_at)
    WHERE completed_at IS NULL AND failed_at IS NULL AND picked_up_at IS NOT NULL;

-- RLS (cascades from chain_executions via join)
ALTER TABLE event_chain.chain_scheduled_jobs ENABLE ROW LEVEL SECURITY;

CREATE POLICY chain_scheduled_jobs_family_policy
    ON event_chain.chain_scheduled_jobs
    FOR ALL
    USING (chain_execution_id IN (
        SELECT id FROM event_chain.chain_executions
        WHERE family_id::text = current_setting('app.current_family_id', true)
    ));
```

### Scheduler Query Pattern

```sql
-- Pick up ready jobs (concurrent-safe)
SELECT *
FROM event_chain.chain_scheduled_jobs
WHERE scheduled_at <= now()
  AND picked_up_at IS NULL
  AND completed_at IS NULL
  AND failed_at IS NULL
ORDER BY scheduled_at ASC
LIMIT 10
FOR UPDATE SKIP LOCKED;

-- Detect stale jobs (picked up but not completed within TTL)
SELECT *
FROM event_chain.chain_scheduled_jobs
WHERE picked_up_at IS NOT NULL
  AND completed_at IS NULL
  AND failed_at IS NULL
  AND picked_up_at < now() - INTERVAL '5 minutes'
FOR UPDATE SKIP LOCKED;
```

---

## EF Core Configuration Summary

### C# Entity → Table Mapping

| C# Entity | Table | Schema |
|-----------|-------|--------|
| `ChainDefinition` | `chain_definitions` | `event_chain` |
| `ChainDefinitionStep` | `chain_definition_steps` | `event_chain` |
| `ChainExecution` | `chain_executions` | `event_chain` |
| `StepExecution` | `step_executions` | `event_chain` |
| `ChainEntityMapping` | `chain_entity_mappings` | `event_chain` |
| `ChainScheduledJob` | `chain_scheduled_jobs` | `event_chain` |

### Vogen Value Object Conversions

| Property | Vogen Type | Database Column |
|----------|-----------|-----------------|
| `ChainDefinition.Id` | `ChainDefinitionId` | `id` (UUID) |
| `ChainExecution.Id` | `ChainExecutionId` | `id` (UUID) |
| `StepExecution.StepAlias` | `StepAlias` | `step_alias` (VARCHAR) |
| `ChainDefinitionStep.Alias` | `StepAlias` | `alias` (VARCHAR) |
| `ChainDefinitionStep.ActionVersion` | `ActionVersion` | `action_version` (VARCHAR) |
| `*.FamilyId` | `FamilyId` | `family_id` (UUID) |
| `*.UserId` | `UserId` | various (UUID) |

### Migration Files

1. `AddEventChainSchema` — Creates schema + all 6 tables + indexes
2. `AddEventChainRlsPolicies` — Enables RLS + creates all 6 policies

---

## Data Flow Diagram

```
Domain Event Published (e.g., HealthAppointmentScheduledEvent)
    │
    ▼
chain_definitions (lookup by trigger_event_type + is_enabled + family_id)
    │
    ▼
chain_executions (INSERT: new saga instance with trigger_payload)
    │
    ▼
step_executions (INSERT: one per step from chain_definition_steps)
    │
    ├─ Immediate steps → Execute now via middleware pipeline
    │   └─ On success → UPDATE status='Completed', output_payload={...}
    │   └─ On failure → UPDATE status='Failed', trigger compensation
    │
    └─ Delayed steps → INSERT into chain_scheduled_jobs
        └─ Scheduler picks up → Execute via middleware pipeline
    │
    ▼
chain_entity_mappings (INSERT: for each entity created by a step)
    │
    ▼
chain_executions (UPDATE: status='Completed' when all steps done)
```

---

## Consistency Notes

- **All timestamps are `TIMESTAMPTZ`** (timezone-aware) — matches existing codebase pattern
- **All UUIDs use `gen_random_uuid()`** — PostgreSQL native, no application-generated defaults
- **JSONB for flexible payloads** — trigger payloads, step inputs/outputs, context
- **CHECK constraints on status columns** — database-level state machine validation
- **CASCADE deletes on child tables** — deleting a chain definition cleans up everything
- **No FK to tables in other schemas** — cross-module references use UUID values only
