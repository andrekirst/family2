-- Avatar module: schema, avatars, avatar_variants tables
CREATE SCHEMA IF NOT EXISTS avatar;

CREATE TABLE IF NOT EXISTS avatar.avatars (
    id uuid NOT NULL,
    original_file_name character varying(255) NOT NULL,
    original_mime_type character varying(50) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_avatars PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS avatar.avatar_variants (
    id uuid NOT NULL,
    avatar_id uuid NOT NULL,
    size character varying(20) NOT NULL,
    storage_key character varying(255) NOT NULL,
    mime_type character varying(50) NOT NULL,
    file_size integer NOT NULL,
    width integer NOT NULL,
    height integer NOT NULL,
    CONSTRAINT pk_avatar_variants PRIMARY KEY (id),
    CONSTRAINT fk_avatar_variants_avatars_avatar_id FOREIGN KEY (avatar_id)
        REFERENCES avatar.avatars (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_avatar_variants_avatar_id_size ON avatar.avatar_variants (avatar_id, size);
