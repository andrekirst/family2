-- Messaging module: schema, messages table
CREATE SCHEMA IF NOT EXISTS messaging;

CREATE TABLE IF NOT EXISTS messaging.messages (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    sender_id uuid NOT NULL,
    content character varying(4000) NOT NULL,
    sent_at timestamp with time zone NOT NULL DEFAULT NOW(),
    conversation_id uuid,
    CONSTRAINT pk_messages PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_messages_family_id_sent_at ON messaging.messages (family_id, sent_at DESC);
