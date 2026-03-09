-- Audit events table for domain event persistence
-- Populated by AuditEventHandler subscribing to all domain event topics.
-- Provides full traceability: who did what, when, and why.
CREATE TABLE IF NOT EXISTS public.audit_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type TEXT NOT NULL,
    event_id UUID NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL,
    actor_user_id UUID,
    correlation_id TEXT,
    causation_id UUID,
    entity_type TEXT,
    entity_id TEXT,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_audit_events_event_type ON public.audit_events (event_type);
CREATE INDEX IF NOT EXISTS ix_audit_events_actor_user_id ON public.audit_events (actor_user_id) WHERE actor_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_audit_events_entity ON public.audit_events (entity_type, entity_id) WHERE entity_type IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_audit_events_correlation_id ON public.audit_events (correlation_id) WHERE correlation_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_audit_events_occurred_at ON public.audit_events (occurred_at);

COMMENT ON TABLE public.audit_events IS 'Immutable audit log of all domain events with actor context and event lineage.';
