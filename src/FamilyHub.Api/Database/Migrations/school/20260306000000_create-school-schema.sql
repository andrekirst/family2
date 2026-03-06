CREATE SCHEMA IF NOT EXISTS school;

CREATE TABLE IF NOT EXISTS school.students (
    id uuid NOT NULL,
    family_member_id uuid NOT NULL,
    family_id uuid NOT NULL,
    marked_by_user_id uuid NOT NULL,
    marked_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_students PRIMARY KEY (id),
    CONSTRAINT uq_students_family_member_id UNIQUE (family_member_id),
    CONSTRAINT fk_students_family_member_id FOREIGN KEY (family_member_id) REFERENCES family.family_members (id) ON DELETE CASCADE,
    CONSTRAINT fk_students_family_id FOREIGN KEY (family_id) REFERENCES family.families (id) ON DELETE CASCADE,
    CONSTRAINT fk_students_marked_by_user_id FOREIGN KEY (marked_by_user_id) REFERENCES auth.users (id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_students_family_id ON school.students (family_id);
CREATE INDEX IF NOT EXISTS ix_students_marked_by_user_id ON school.students (marked_by_user_id);
