-- Messaging module: message_attachments table (with storage_key column included)
CREATE TABLE IF NOT EXISTS messaging.message_attachments (
    id uuid NOT NULL,
    file_id uuid NOT NULL,
    file_name character varying(255) NOT NULL,
    mime_type character varying(127) NOT NULL,
    file_size bigint NOT NULL,
    attached_at timestamp with time zone NOT NULL,
    message_id uuid NOT NULL,
    storage_key character varying(255),
    CONSTRAINT pk_message_attachments PRIMARY KEY (id),
    CONSTRAINT fk_message_attachments_messages_message_id FOREIGN KEY (message_id)
        REFERENCES messaging.messages (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_message_attachments_message_id ON messaging.message_attachments (message_id);
