ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS address_street varchar(200) NULL;
ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS address_house_number varchar(20) NULL;
ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS address_postal_code varchar(20) NULL;
ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS address_city varchar(100) NULL;
ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS address_country varchar(100) NULL;
ALTER TABLE family.family_members ADD COLUMN IF NOT EXISTS address_federal_state_id uuid NULL;
