using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFileManagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "file_management");

            migrationBuilder.CreateTable(
                name: "album_items",
                schema: "file_management",
                columns: table => new
                {
                    album_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_by = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_album_items", x => new { x.album_id, x.file_id });
                });

            migrationBuilder.CreateTable(
                name: "albums",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cover_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_albums", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_connections",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    encrypted_access_token = table.Column<string>(type: "text", nullable: false),
                    encrypted_refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    connected_by = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    connected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_connections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_blobs",
                schema: "file_management",
                columns: table => new
                {
                    storage_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_blobs", x => x.storage_key);
                });

            migrationBuilder.CreateTable(
                name: "file_metadata",
                schema: "file_management",
                columns: table => new
                {
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gps_latitude = table.Column<double>(type: "double precision", nullable: true),
                    gps_longitude = table.Column<double>(type: "double precision", nullable: true),
                    location_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    camera_model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    capture_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    raw_exif = table.Column<string>(type: "jsonb", nullable: true),
                    extracted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_metadata", x => x.file_id);
                });

            migrationBuilder.CreateTable(
                name: "file_permissions",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<int>(type: "integer", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_level = table.Column<int>(type: "integer", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_tags",
                schema: "file_management",
                columns: table => new
                {
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_tags", x => new { x.file_id, x.tag_id });
                });

            migrationBuilder.CreateTable(
                name: "file_thumbnails",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_thumbnails", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_versions",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_versions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "files",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    folder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "folders",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    parent_folder_id = table.Column<Guid>(type: "uuid", nullable: true),
                    materialized_path = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_inbox = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_folders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_rules",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    conditions_json = table.Column<string>(type: "jsonb", nullable: false),
                    condition_logic = table.Column<int>(type: "integer", nullable: false),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    actions_json = table.Column<string>(type: "jsonb", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processing_log",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    matched_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matched_rule_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    action_taken = table.Column<int>(type: "integer", nullable: true),
                    destination_folder_id = table.Column<Guid>(type: "uuid", nullable: true),
                    applied_tag_names = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recent_searches",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    query = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    searched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recent_searches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saved_searches",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    query = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    filters_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_searches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "secure_notes",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    encrypted_title = table.Column<string>(type: "text", nullable: false),
                    encrypted_content = table.Column<string>(type: "text", nullable: false),
                    iv = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    salt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sentinel = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_secure_notes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "share_link_access_log",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    share_link_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    action = table.Column<int>(type: "integer", nullable: false),
                    accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_share_link_access_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "share_links",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    resource_type = table.Column<int>(type: "integer", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    max_downloads = table.Column<int>(type: "integer", nullable: true),
                    download_count = table.Column<int>(type: "integer", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_share_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "storage_quotas",
                schema: "file_management",
                columns: table => new
                {
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    used_bytes = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    max_bytes = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_storage_quotas", x => x.family_id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "upload_chunks",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    upload_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_upload_chunks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_favorites",
                schema: "file_management",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    favorited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_favorites", x => new { x.user_id, x.file_id });
                });

            migrationBuilder.CreateTable(
                name: "zip_jobs",
                schema: "file_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    initiated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    file_ids = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    progress = table.Column<int>(type: "integer", nullable: false),
                    zip_storage_key = table.Column<string>(type: "text", nullable: true),
                    zip_size = table.Column<long>(type: "bigint", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zip_jobs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_album_items_album_id",
                schema: "file_management",
                table: "album_items",
                column: "album_id");

            migrationBuilder.CreateIndex(
                name: "ix_album_items_file_id",
                schema: "file_management",
                table: "album_items",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_albums_family_id",
                schema: "file_management",
                table: "albums",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_connections_family_id",
                schema: "file_management",
                table: "external_connections",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_connections_family_id_provider_type",
                schema: "file_management",
                table: "external_connections",
                columns: new[] { "family_id", "provider_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_permissions_member_id_family_id",
                schema: "file_management",
                table: "file_permissions",
                columns: new[] { "member_id", "family_id" });

            migrationBuilder.CreateIndex(
                name: "ix_file_permissions_member_id_resource_type_resource_id",
                schema: "file_management",
                table: "file_permissions",
                columns: new[] { "member_id", "resource_type", "resource_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_permissions_resource_type_resource_id",
                schema: "file_management",
                table: "file_permissions",
                columns: new[] { "resource_type", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ix_file_thumbnails_file_id",
                schema: "file_management",
                table: "file_thumbnails",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_thumbnails_file_id_width_height",
                schema: "file_management",
                table: "file_thumbnails",
                columns: new[] { "file_id", "width", "height" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_versions_file_id",
                schema: "file_management",
                table: "file_versions",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_versions_file_id_is_current",
                schema: "file_management",
                table: "file_versions",
                columns: new[] { "file_id", "is_current" });

            migrationBuilder.CreateIndex(
                name: "ix_files_family_id",
                schema: "file_management",
                table: "files",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_files_folder_id",
                schema: "file_management",
                table: "files",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "ix_folders_family_id",
                schema: "file_management",
                table: "folders",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_folders_family_id_is_inbox",
                schema: "file_management",
                table: "folders",
                columns: new[] { "family_id", "is_inbox" });

            migrationBuilder.CreateIndex(
                name: "ix_folders_materialized_path",
                schema: "file_management",
                table: "folders",
                column: "materialized_path");

            migrationBuilder.CreateIndex(
                name: "ix_folders_parent_folder_id",
                schema: "file_management",
                table: "folders",
                column: "parent_folder_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_family_id",
                schema: "file_management",
                table: "organization_rules",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_family_id_priority",
                schema: "file_management",
                table: "organization_rules",
                columns: new[] { "family_id", "priority" });

            migrationBuilder.CreateIndex(
                name: "ix_processing_log_family_id",
                schema: "file_management",
                table: "processing_log",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_log_processed_at",
                schema: "file_management",
                table: "processing_log",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_recent_searches_user_id_searched_at",
                schema: "file_management",
                table: "recent_searches",
                columns: new[] { "user_id", "searched_at" });

            migrationBuilder.CreateIndex(
                name: "ix_saved_searches_user_id",
                schema: "file_management",
                table: "saved_searches",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_secure_notes_user_id_family_id",
                schema: "file_management",
                table: "secure_notes",
                columns: new[] { "user_id", "family_id" });

            migrationBuilder.CreateIndex(
                name: "ix_secure_notes_user_id_family_id_category",
                schema: "file_management",
                table: "secure_notes",
                columns: new[] { "user_id", "family_id", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_share_link_access_log_share_link_id",
                schema: "file_management",
                table: "share_link_access_log",
                column: "share_link_id");

            migrationBuilder.CreateIndex(
                name: "ix_share_links_family_id",
                schema: "file_management",
                table: "share_links",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_share_links_resource_id",
                schema: "file_management",
                table: "share_links",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_share_links_token",
                schema: "file_management",
                table: "share_links",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_family_id_name",
                schema: "file_management",
                table: "tags",
                columns: new[] { "family_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_upload_chunks_upload_id_chunk_index",
                schema: "file_management",
                table: "upload_chunks",
                columns: new[] { "upload_id", "chunk_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_favorites_file_id",
                schema: "file_management",
                table: "user_favorites",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_favorites_user_id",
                schema: "file_management",
                table: "user_favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_zip_jobs_expires_at",
                schema: "file_management",
                table: "zip_jobs",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_zip_jobs_family_id",
                schema: "file_management",
                table: "zip_jobs",
                column: "family_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "album_items",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "albums",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "external_connections",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "file_blobs",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "file_metadata",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "file_permissions",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "file_tags",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "file_thumbnails",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "file_versions",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "files",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "folders",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "organization_rules",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "processing_log",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "recent_searches",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "saved_searches",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "secure_notes",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "share_link_access_log",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "share_links",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "storage_quotas",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "upload_chunks",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "user_favorites",
                schema: "file_management");

            migrationBuilder.DropTable(
                name: "zip_jobs",
                schema: "file_management");
        }
    }
}
