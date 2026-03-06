-- File Management module: schema and all tables
CREATE SCHEMA IF NOT EXISTS file_management;

-- Folders
CREATE TABLE IF NOT EXISTS file_management.folders (
    id uuid NOT NULL,
    name character varying(255) NOT NULL,
    parent_folder_id uuid,
    materialized_path character varying(4000) NOT NULL,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    is_inbox boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_folders PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_folders_family_id ON file_management.folders (family_id);
CREATE INDEX IF NOT EXISTS ix_folders_family_id_is_inbox ON file_management.folders (family_id, is_inbox);
CREATE INDEX IF NOT EXISTS ix_folders_materialized_path ON file_management.folders (materialized_path);
CREATE INDEX IF NOT EXISTS ix_folders_parent_folder_id ON file_management.folders (parent_folder_id);

-- Files
CREATE TABLE IF NOT EXISTS file_management.files (
    id uuid NOT NULL,
    name character varying(255) NOT NULL,
    mime_type character varying(255) NOT NULL,
    size bigint NOT NULL,
    storage_key character varying(255) NOT NULL,
    checksum character varying(64) NOT NULL,
    folder_id uuid NOT NULL,
    family_id uuid NOT NULL,
    uploaded_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_files PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_files_family_id ON file_management.files (family_id);
CREATE INDEX IF NOT EXISTS ix_files_folder_id ON file_management.files (folder_id);

-- File Blobs
CREATE TABLE IF NOT EXISTS file_management.file_blobs (
    storage_key character varying(255) NOT NULL,
    data bytea NOT NULL,
    mime_type character varying(255) NOT NULL,
    size bigint NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_file_blobs PRIMARY KEY (storage_key)
);

-- File Metadata
CREATE TABLE IF NOT EXISTS file_management.file_metadata (
    file_id uuid NOT NULL,
    gps_latitude double precision,
    gps_longitude double precision,
    location_name character varying(500),
    camera_model character varying(200),
    capture_date timestamp with time zone,
    raw_exif jsonb,
    extracted_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_file_metadata PRIMARY KEY (file_id)
);

-- File Versions
CREATE TABLE IF NOT EXISTS file_management.file_versions (
    id uuid NOT NULL,
    file_id uuid NOT NULL,
    version_number integer NOT NULL,
    storage_key character varying(255) NOT NULL,
    file_size bigint NOT NULL,
    checksum character varying(64) NOT NULL,
    uploaded_by uuid NOT NULL,
    is_current boolean NOT NULL,
    uploaded_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_file_versions PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_file_versions_file_id ON file_management.file_versions (file_id);
CREATE INDEX IF NOT EXISTS ix_file_versions_file_id_is_current ON file_management.file_versions (file_id, is_current);

-- File Thumbnails
CREATE TABLE IF NOT EXISTS file_management.file_thumbnails (
    id uuid NOT NULL,
    file_id uuid NOT NULL,
    width integer NOT NULL,
    height integer NOT NULL,
    storage_key character varying(255) NOT NULL,
    generated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_file_thumbnails PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_file_thumbnails_file_id ON file_management.file_thumbnails (file_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_file_thumbnails_file_id_width_height ON file_management.file_thumbnails (file_id, width, height);

-- Tags
CREATE TABLE IF NOT EXISTS file_management.tags (
    id uuid NOT NULL,
    name character varying(50) NOT NULL,
    color character varying(7) NOT NULL,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_tags PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_tags_family_id_name ON file_management.tags (family_id, name);

-- File Tags (join table)
CREATE TABLE IF NOT EXISTS file_management.file_tags (
    file_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_file_tags PRIMARY KEY (file_id, tag_id)
);

-- Albums
CREATE TABLE IF NOT EXISTS file_management.albums (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(500),
    cover_file_id uuid,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_albums PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_albums_family_id ON file_management.albums (family_id);

-- Album Items (join table)
CREATE TABLE IF NOT EXISTS file_management.album_items (
    album_id uuid NOT NULL,
    file_id uuid NOT NULL,
    added_by uuid NOT NULL,
    added_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_album_items PRIMARY KEY (album_id, file_id)
);

CREATE INDEX IF NOT EXISTS ix_album_items_album_id ON file_management.album_items (album_id);
CREATE INDEX IF NOT EXISTS ix_album_items_file_id ON file_management.album_items (file_id);

-- File Permissions
CREATE TABLE IF NOT EXISTS file_management.file_permissions (
    id uuid NOT NULL,
    resource_type integer NOT NULL,
    resource_id uuid NOT NULL,
    member_id uuid NOT NULL,
    permission_level integer NOT NULL,
    family_id uuid NOT NULL,
    granted_by uuid NOT NULL,
    granted_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_file_permissions PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_file_permissions_member_id_family_id ON file_management.file_permissions (member_id, family_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_file_permissions_member_id_resource_type_resource_id ON file_management.file_permissions (member_id, resource_type, resource_id);
CREATE INDEX IF NOT EXISTS ix_file_permissions_resource_type_resource_id ON file_management.file_permissions (resource_type, resource_id);

-- Share Links
CREATE TABLE IF NOT EXISTS file_management.share_links (
    id uuid NOT NULL,
    token character varying(64) NOT NULL,
    resource_type integer NOT NULL,
    resource_id uuid NOT NULL,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    expires_at timestamp with time zone,
    password_hash character varying(255),
    max_downloads integer,
    download_count integer NOT NULL,
    is_revoked boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_share_links PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_share_links_family_id ON file_management.share_links (family_id);
CREATE INDEX IF NOT EXISTS ix_share_links_resource_id ON file_management.share_links (resource_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_share_links_token ON file_management.share_links (token);

-- Share Link Access Log
CREATE TABLE IF NOT EXISTS file_management.share_link_access_log (
    id uuid NOT NULL,
    share_link_id uuid NOT NULL,
    ip_address character varying(45) NOT NULL,
    user_agent character varying(512),
    action integer NOT NULL,
    accessed_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_share_link_access_log PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_share_link_access_log_share_link_id ON file_management.share_link_access_log (share_link_id);

-- External Connections
CREATE TABLE IF NOT EXISTS file_management.external_connections (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    provider_type character varying(50) NOT NULL,
    display_name character varying(200) NOT NULL,
    encrypted_access_token text NOT NULL,
    encrypted_refresh_token text,
    token_expires_at timestamp with time zone,
    connected_by uuid NOT NULL,
    status character varying(50) NOT NULL,
    connected_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_external_connections PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_external_connections_family_id ON file_management.external_connections (family_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_external_connections_family_id_provider_type ON file_management.external_connections (family_id, provider_type);

-- Organization Rules
CREATE TABLE IF NOT EXISTS file_management.organization_rules (
    id uuid NOT NULL,
    name character varying(200) NOT NULL,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    conditions_json jsonb NOT NULL,
    condition_logic integer NOT NULL,
    action_type integer NOT NULL,
    actions_json jsonb NOT NULL,
    priority integer NOT NULL,
    is_enabled boolean NOT NULL DEFAULT true,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_organization_rules PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_organization_rules_family_id ON file_management.organization_rules (family_id);
CREATE INDEX IF NOT EXISTS ix_organization_rules_family_id_priority ON file_management.organization_rules (family_id, priority);

-- Processing Log
CREATE TABLE IF NOT EXISTS file_management.processing_log (
    id uuid NOT NULL,
    file_id uuid NOT NULL,
    file_name character varying(255) NOT NULL,
    matched_rule_id uuid,
    matched_rule_name character varying(200),
    action_taken integer,
    destination_folder_id uuid,
    applied_tag_names character varying(1000),
    success boolean NOT NULL,
    error_message character varying(2000),
    family_id uuid NOT NULL,
    processed_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_processing_log PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_processing_log_family_id ON file_management.processing_log (family_id);
CREATE INDEX IF NOT EXISTS ix_processing_log_processed_at ON file_management.processing_log (processed_at);

-- Storage Quotas
CREATE TABLE IF NOT EXISTS file_management.storage_quotas (
    family_id uuid NOT NULL,
    used_bytes bigint NOT NULL DEFAULT 0,
    max_bytes bigint NOT NULL,
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_storage_quotas PRIMARY KEY (family_id)
);

-- Upload Chunks
CREATE TABLE IF NOT EXISTS file_management.upload_chunks (
    id uuid NOT NULL,
    upload_id character varying(255) NOT NULL,
    chunk_index integer NOT NULL,
    data bytea NOT NULL,
    size bigint NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_upload_chunks PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_upload_chunks_upload_id_chunk_index ON file_management.upload_chunks (upload_id, chunk_index);

-- Zip Jobs
CREATE TABLE IF NOT EXISTS file_management.zip_jobs (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    initiated_by uuid NOT NULL,
    file_ids jsonb NOT NULL,
    status character varying(50) NOT NULL,
    progress integer NOT NULL,
    zip_storage_key text,
    zip_size bigint,
    error_message text,
    created_at timestamp with time zone NOT NULL,
    completed_at timestamp with time zone,
    expires_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_zip_jobs PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_zip_jobs_expires_at ON file_management.zip_jobs (expires_at);
CREATE INDEX IF NOT EXISTS ix_zip_jobs_family_id ON file_management.zip_jobs (family_id);

-- User Favorites
CREATE TABLE IF NOT EXISTS file_management.user_favorites (
    user_id uuid NOT NULL,
    file_id uuid NOT NULL,
    favorited_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_user_favorites PRIMARY KEY (user_id, file_id)
);

CREATE INDEX IF NOT EXISTS ix_user_favorites_file_id ON file_management.user_favorites (file_id);
CREATE INDEX IF NOT EXISTS ix_user_favorites_user_id ON file_management.user_favorites (user_id);

-- Secure Notes
CREATE TABLE IF NOT EXISTS file_management.secure_notes (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    user_id uuid NOT NULL,
    category character varying(50) NOT NULL,
    encrypted_title text NOT NULL,
    encrypted_content text NOT NULL,
    iv character varying(100) NOT NULL,
    salt character varying(100) NOT NULL,
    sentinel text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_secure_notes PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_secure_notes_user_id_family_id ON file_management.secure_notes (user_id, family_id);
CREATE INDEX IF NOT EXISTS ix_secure_notes_user_id_family_id_category ON file_management.secure_notes (user_id, family_id, category);

-- Recent Searches
CREATE TABLE IF NOT EXISTS file_management.recent_searches (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    query character varying(500) NOT NULL,
    searched_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_recent_searches PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_recent_searches_user_id_searched_at ON file_management.recent_searches (user_id, searched_at);

-- Saved Searches
CREATE TABLE IF NOT EXISTS file_management.saved_searches (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(200) NOT NULL,
    query character varying(500) NOT NULL,
    filters_json jsonb,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_saved_searches PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_saved_searches_user_id ON file_management.saved_searches (user_id);
