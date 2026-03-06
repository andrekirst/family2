-- Family module: schema, families, family_members, family_invitations tables
CREATE SCHEMA IF NOT EXISTS family;

CREATE TABLE IF NOT EXISTS family.families (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    owner_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_families PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_families_owner_id ON family.families (owner_id);

CREATE TABLE IF NOT EXISTS family.family_members (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    user_id uuid NOT NULL,
    role character varying(20) NOT NULL,
    joined_at timestamp with time zone NOT NULL DEFAULT NOW(),
    is_active boolean NOT NULL DEFAULT true,
    CONSTRAINT pk_family_members PRIMARY KEY (id),
    CONSTRAINT fk_family_members_families_family_id FOREIGN KEY (family_id)
        REFERENCES family.families (id) ON DELETE CASCADE,
    CONSTRAINT fk_family_members_users_user_id FOREIGN KEY (user_id)
        REFERENCES auth.users (id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_family_members_family_id_user_id ON family.family_members (family_id, user_id);
CREATE INDEX IF NOT EXISTS ix_family_members_user_id ON family.family_members (user_id);

CREATE TABLE IF NOT EXISTS family.family_invitations (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    invited_by_user_id uuid NOT NULL,
    invitee_email character varying(320) NOT NULL,
    token_hash character varying(64) NOT NULL,
    role character varying(20) NOT NULL,
    status character varying(20) NOT NULL DEFAULT 'Pending',
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    expires_at timestamp with time zone NOT NULL,
    accepted_by_user_id uuid,
    accepted_at timestamp with time zone,
    CONSTRAINT pk_family_invitations PRIMARY KEY (id),
    CONSTRAINT fk_family_invitations_families_family_id FOREIGN KEY (family_id)
        REFERENCES family.families (id) ON DELETE CASCADE,
    CONSTRAINT fk_family_invitations_users_accepted_by_user_id FOREIGN KEY (accepted_by_user_id)
        REFERENCES auth.users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_family_invitations_users_invited_by_user_id FOREIGN KEY (invited_by_user_id)
        REFERENCES auth.users (id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_family_invitations_accepted_by_user_id ON family.family_invitations (accepted_by_user_id);
CREATE INDEX IF NOT EXISTS ix_family_invitations_family_id_status ON family.family_invitations (family_id, status);
CREATE INDEX IF NOT EXISTS ix_family_invitations_invited_by_user_id ON family.family_invitations (invited_by_user_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_family_invitations_token_hash ON family.family_invitations (token_hash);
