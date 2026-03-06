-- Bridge script: EF Core → DbUp migration transition.
--
-- On existing EF Core databases (__EFMigrationsHistory exists):
--   Pre-registers all initial DbUp scripts in the schemaversions journal
--   so they are skipped (schemas already exist from EF Core migrations).
--
-- On fresh databases (__EFMigrationsHistory does not exist):
--   Does nothing — all scripts will run normally to create the schema.

DO $$
DECLARE
    scripts text[] := ARRAY[
        'FamilyHub.Api.Database.Migrations.auth.20260211103143_create-auth-schema.sql',
        'FamilyHub.Api.Database.Migrations.auth.20260213152858_add-user-preferred-locale.sql',
        'FamilyHub.Api.Database.Migrations.auth.20260213170331_add-user-avatar-id.sql',
        'FamilyHub.Api.Database.Migrations.avatar.20260213170331_create-avatar-schema.sql',
        'FamilyHub.Api.Database.Migrations.calendar.20260211103143_create-calendar-schema.sql',
        'FamilyHub.Api.Database.Migrations.dashboard.20260216203932_create-dashboard-schema.sql',
        'FamilyHub.Api.Database.Migrations.event_chain.20260211103143_create-event-chain-schema.sql',
        'FamilyHub.Api.Database.Migrations.family.20260211103143_create-family-schema.sql',
        'FamilyHub.Api.Database.Migrations.family.20260213170331_add-family-member-avatar-id.sql',
        'FamilyHub.Api.Database.Migrations.file_management.20260224072504_create-file-management-schema.sql',
        'FamilyHub.Api.Database.Migrations.google_integration.20260213211010_create-google-integration-schema.sql',
        'FamilyHub.Api.Database.Migrations.messaging.20260227190445_create-messaging-schema.sql',
        'FamilyHub.Api.Database.Migrations.messaging.20260303125924_add-message-attachments.sql',
        'FamilyHub.Api.Database.Migrations.messaging.20260304102846_add-conversations.sql',
        'FamilyHub.Api.Database.Migrations.photos.20260227085532_create-photos-schema.sql',
        'FamilyHub.Api.Database.Migrations.rls.20260211103239_add-rls-policies.sql',
        'FamilyHub.Api.Database.Migrations.rls.20260217114809_add-google-integration-rls-policies.sql',
        'FamilyHub.Api.Database.Migrations.storage.20260213170331_create-storage-schema.sql',
        'FamilyHub.Api.Database.Migrations.x_cross_schema.20260211103143_add-cross-schema-foreign-keys.sql'
    ];
    script_name text;
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory'
    ) THEN
        -- This database was managed by EF Core. Pre-register all initial scripts
        -- so DbUp skips them (the schemas already exist).
        FOREACH script_name IN ARRAY scripts
        LOOP
            IF NOT EXISTS (
                SELECT 1 FROM public.schemaversions WHERE scriptname = script_name
            ) THEN
                INSERT INTO public.schemaversions (scriptname, applied)
                VALUES (script_name, NOW());
            END IF;
        END LOOP;

        -- Clean up the legacy EF Core migration history table
        DROP TABLE public."__EFMigrationsHistory";

        RAISE NOTICE 'Bridge: EF Core database detected. Pre-registered % scripts in schemaversions and dropped __EFMigrationsHistory.', array_length(scripts, 1);
    ELSE
        RAISE NOTICE 'Bridge: Fresh database detected. All scripts will run normally.';
    END IF;
END $$;
