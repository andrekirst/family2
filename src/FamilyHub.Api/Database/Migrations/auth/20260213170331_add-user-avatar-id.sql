-- Auth module: add avatar_id column to users
ALTER TABLE auth.users ADD COLUMN IF NOT EXISTS avatar_id uuid;
