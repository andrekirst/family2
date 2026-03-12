CREATE TABLE IF NOT EXISTS school.class_assignments (
    id uuid NOT NULL,
    student_id uuid NOT NULL,
    school_id uuid NOT NULL,
    school_year_id uuid NOT NULL,
    class_name varchar(20) NOT NULL,
    family_id uuid NOT NULL,
    assigned_by_user_id uuid NOT NULL,
    assigned_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_class_assignments PRIMARY KEY (id),
    CONSTRAINT fk_class_assignments_student_id FOREIGN KEY (student_id) REFERENCES school.students (id) ON DELETE CASCADE,
    CONSTRAINT fk_class_assignments_school_id FOREIGN KEY (school_id) REFERENCES school.schools (id) ON DELETE RESTRICT,
    CONSTRAINT fk_class_assignments_school_year_id FOREIGN KEY (school_year_id) REFERENCES school.school_years (id) ON DELETE RESTRICT,
    CONSTRAINT fk_class_assignments_family_id FOREIGN KEY (family_id) REFERENCES family.families (id) ON DELETE CASCADE,
    CONSTRAINT fk_class_assignments_assigned_by_user_id FOREIGN KEY (assigned_by_user_id) REFERENCES auth.users (id) ON DELETE RESTRICT,
    CONSTRAINT uq_class_assignments_student_school_year UNIQUE (student_id, school_year_id)
);

CREATE INDEX IF NOT EXISTS ix_class_assignments_student_id ON school.class_assignments (student_id);
CREATE INDEX IF NOT EXISTS ix_class_assignments_family_id ON school.class_assignments (family_id);
