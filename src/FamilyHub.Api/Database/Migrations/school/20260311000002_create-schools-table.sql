CREATE TABLE IF NOT EXISTS school.schools (
    id uuid NOT NULL,
    name varchar(200) NOT NULL,
    family_id uuid NOT NULL,
    federal_state_id uuid NOT NULL,
    city varchar(100) NOT NULL,
    postal_code varchar(20) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_schools PRIMARY KEY (id),
    CONSTRAINT fk_schools_family_id FOREIGN KEY (family_id) REFERENCES family.families (id) ON DELETE CASCADE,
    CONSTRAINT fk_schools_federal_state_id FOREIGN KEY (federal_state_id) REFERENCES base_data.federal_states (id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_schools_family_id ON school.schools (family_id);
