-- Cross-schema foreign keys that create circular dependencies between modules.
-- These must run AFTER both auth and family schemas are created.

-- auth.users.family_id → family.families.id
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_users_families_family_id'
    ) THEN
        ALTER TABLE auth.users
            ADD CONSTRAINT fk_users_families_family_id
            FOREIGN KEY (family_id) REFERENCES family.families (id) ON DELETE SET NULL;
    END IF;
END $$;

-- family.families.owner_id → auth.users.id
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_families_users_owner_id'
    ) THEN
        ALTER TABLE family.families
            ADD CONSTRAINT fk_families_users_owner_id
            FOREIGN KEY (owner_id) REFERENCES auth.users (id) ON DELETE RESTRICT;
    END IF;
END $$;
