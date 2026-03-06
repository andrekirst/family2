-- Messaging module: conversations and conversation_members tables, message index
CREATE TABLE IF NOT EXISTS messaging.conversations (
    id uuid NOT NULL,
    name character varying(255) NOT NULL,
    type character varying(20) NOT NULL,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    folder_id uuid,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_conversations PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_conversations_family_id_type ON messaging.conversations (family_id, type);

CREATE TABLE IF NOT EXISTS messaging.conversation_members (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    role character varying(50) NOT NULL,
    joined_at timestamp with time zone NOT NULL,
    left_at timestamp with time zone,
    conversation_id uuid NOT NULL,
    CONSTRAINT pk_conversation_members PRIMARY KEY (id),
    CONSTRAINT fk_conversation_members_conversations_conversation_id FOREIGN KEY (conversation_id)
        REFERENCES messaging.conversations (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_conversation_members_conversation_id ON messaging.conversation_members (conversation_id);
CREATE INDEX IF NOT EXISTS ix_conversation_members_user_id ON messaging.conversation_members (user_id);

CREATE INDEX IF NOT EXISTS ix_messages_conversation_id_sent_at ON messaging.messages (conversation_id, sent_at DESC);
