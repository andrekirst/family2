-- Photos module: schema, photos table
CREATE SCHEMA IF NOT EXISTS photos;

CREATE TABLE IF NOT EXISTS photos.photos (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    uploaded_by uuid NOT NULL,
    file_name character varying(500) NOT NULL,
    content_type character varying(100) NOT NULL,
    file_size_bytes bigint NOT NULL,
    storage_path character varying(1000) NOT NULL,
    caption character varying(500),
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_photos PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_photos_family_id_created_at ON photos.photos (family_id, created_at);
CREATE INDEX IF NOT EXISTS ix_photos_family_id_is_deleted ON photos.photos (family_id, is_deleted);
