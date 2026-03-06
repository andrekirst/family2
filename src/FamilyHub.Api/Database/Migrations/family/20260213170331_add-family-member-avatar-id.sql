-- Family module: add avatar_id column to family_members
ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS avatar_id uuid;
