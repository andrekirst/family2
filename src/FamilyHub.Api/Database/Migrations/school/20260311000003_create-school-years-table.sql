CREATE TABLE IF NOT EXISTS school.school_years (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    federal_state_id uuid NOT NULL,
    start_year integer NOT NULL,
    end_year integer NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_school_years PRIMARY KEY (id),
    CONSTRAINT fk_school_years_family_id FOREIGN KEY (family_id) REFERENCES family.families (id) ON DELETE CASCADE,
    CONSTRAINT fk_school_years_federal_state_id FOREIGN KEY (federal_state_id) REFERENCES base_data.federal_states (id) ON DELETE RESTRICT,
    CONSTRAINT uq_school_years_family_state_year UNIQUE (family_id, federal_state_id, start_year, end_year)
);

CREATE INDEX IF NOT EXISTS ix_school_years_family_id ON school.school_years (family_id);
