CREATE SCHEMA IF NOT EXISTS common;

CREATE TABLE IF NOT EXISTS common.blob_staging (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module TEXT NOT NULL,
    storage_key TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'pending',
    retry_count INT NOT NULL DEFAULT 0,
    max_retries INT NOT NULL DEFAULT 5,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    promoted_at TIMESTAMPTZ,
    error_message TEXT,
    metadata JSONB
);

CREATE INDEX IF NOT EXISTS ix_blob_staging_status ON common.blob_staging(status) WHERE status = 'pending';
