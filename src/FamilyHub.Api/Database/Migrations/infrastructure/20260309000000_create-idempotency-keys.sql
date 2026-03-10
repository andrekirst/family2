-- Idempotency keys table for request deduplication
-- Used by IdempotencyBehavior to prevent duplicate command execution
-- when CAP delivers at-least-once or clients retry failed requests.
CREATE TABLE IF NOT EXISTS public.idempotency_keys (
    key_hash TEXT PRIMARY KEY,
    result_json JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL DEFAULT NOW() + INTERVAL '24 hours'
);

CREATE INDEX IF NOT EXISTS ix_idempotency_keys_expires_at
    ON public.idempotency_keys (expires_at);

COMMENT ON TABLE public.idempotency_keys IS 'Request deduplication for at-least-once delivery. Entries expire after 24 hours.';
