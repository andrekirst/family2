-- Dashboard module: schema, dashboard_layouts, dashboard_widgets tables
CREATE SCHEMA IF NOT EXISTS dashboard;

CREATE TABLE IF NOT EXISTS dashboard.dashboard_layouts (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    user_id uuid,
    family_id uuid,
    is_shared boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_dashboard_layouts PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_dashboard_layouts_family_id ON dashboard.dashboard_layouts (family_id) WHERE family_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_dashboard_layouts_user_id ON dashboard.dashboard_layouts (user_id) WHERE user_id IS NOT NULL;

CREATE TABLE IF NOT EXISTS dashboard.dashboard_widgets (
    id uuid NOT NULL,
    dashboard_id uuid NOT NULL,
    widget_type character varying(100) NOT NULL,
    x integer NOT NULL,
    y integer NOT NULL,
    width integer NOT NULL,
    height integer NOT NULL,
    sort_order integer NOT NULL DEFAULT 0,
    config_json jsonb,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_dashboard_widgets PRIMARY KEY (id),
    CONSTRAINT fk_dashboard_widgets_dashboard_layouts_dashboard_id FOREIGN KEY (dashboard_id)
        REFERENCES dashboard.dashboard_layouts (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_dashboard_widgets_dashboard_id ON dashboard.dashboard_widgets (dashboard_id);
