-- Phase 2, Workstream D: Quartz.NET Background Jobs - Manual Migration
-- Create table for QueuedManagedAccountCreation entity

-- Create table
CREATE TABLE auth.queued_managed_account_creations (
    id UUID PRIMARY KEY,
    family_id UUID NOT NULL,
    username VARCHAR(20) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role VARCHAR(50) NOT NULL,
    encrypted_password VARCHAR(500) NOT NULL,
    created_by_user_id UUID NOT NULL,
    retry_count INT NOT NULL DEFAULT 0,
    status VARCHAR(50) NOT NULL,
    next_retry_at TIMESTAMP NULL,
    last_error_message VARCHAR(2000) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_queued_managed_account_creations_family_id
        FOREIGN KEY (family_id)
        REFERENCES auth.families(id)
        ON DELETE RESTRICT
);

-- Create indexes
CREATE INDEX ix_queued_managed_account_creations_family_id
    ON auth.queued_managed_account_creations(family_id);

CREATE INDEX ix_queued_managed_account_creations_status
    ON auth.queued_managed_account_creations(status);

CREATE INDEX ix_queued_managed_account_creations_status_next_retry
    ON auth.queued_managed_account_creations(status, next_retry_at);

-- Add comments
COMMENT ON TABLE auth.queued_managed_account_creations IS
    'Queue for managed account creations with retry logic (exponential backoff: 1min, 5min, 15min, 1hr, 4hr)';

COMMENT ON COLUMN auth.queued_managed_account_creations.encrypted_password IS
    'CRITICAL: Password must be encrypted before storage using Data Protection API';

COMMENT ON COLUMN auth.queued_managed_account_creations.retry_count IS
    'Number of retry attempts (max 5 attempts before permanent failure)';

COMMENT ON COLUMN auth.queued_managed_account_creations.next_retry_at IS
    'Scheduled time for next retry attempt (NULL if job completed or permanently failed)';
