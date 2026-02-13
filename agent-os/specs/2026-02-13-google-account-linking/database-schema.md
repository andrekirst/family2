# Database Schema — Google Account Linking

## Schema: `google_integration`

### Table: `google_account_links`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | `UUID` | PK, DEFAULT gen_random_uuid() | Aggregate ID |
| `user_id` | `UUID` | NOT NULL, UNIQUE | FK concept to auth.users |
| `google_account_id` | `VARCHAR(255)` | NOT NULL, UNIQUE | Google "sub" claim |
| `google_email` | `VARCHAR(320)` | NOT NULL | Google account email |
| `encrypted_access_token` | `TEXT` | NOT NULL | AES-256-GCM ciphertext (base64) |
| `encrypted_refresh_token` | `TEXT` | NOT NULL | AES-256-GCM ciphertext (base64) |
| `access_token_expires_at` | `TIMESTAMPTZ` | NOT NULL | When access token expires |
| `granted_scopes` | `TEXT` | NOT NULL | Space-separated scope list |
| `status` | `VARCHAR(50)` | NOT NULL, DEFAULT 'Active' | Active, Revoked, Expired, Error |
| `last_sync_at` | `TIMESTAMPTZ` | NULL | Last successful API sync |
| `last_error` | `TEXT` | NULL | Last error message |
| `created_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT NOW() | Record creation |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT NOW() | Last update |

### Indexes

```sql
CREATE UNIQUE INDEX ix_google_account_links_user_id
    ON google_integration.google_account_links (user_id);

CREATE UNIQUE INDEX ix_google_account_links_google_account_id
    ON google_integration.google_account_links (google_account_id);

CREATE INDEX ix_google_account_links_status
    ON google_integration.google_account_links (status);

CREATE INDEX ix_google_account_links_expires
    ON google_integration.google_account_links (access_token_expires_at)
    WHERE status = 'Active';
```

### RLS Policy

```sql
ALTER TABLE google_integration.google_account_links ENABLE ROW LEVEL SECURITY;

CREATE POLICY google_account_links_user_policy
    ON google_integration.google_account_links
    USING (user_id = current_setting('app.current_user_id', true)::uuid);
```

### Table: `oauth_states` (ephemeral)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `state` | `VARCHAR(128)` | PK | Cryptographic random state |
| `user_id` | `UUID` | NOT NULL | User who initiated the flow |
| `code_verifier` | `VARCHAR(128)` | NOT NULL | PKCE code verifier |
| `created_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT NOW() | Creation time |
| `expires_at` | `TIMESTAMPTZ` | NOT NULL | Expiry (10 min from creation) |

```sql
CREATE INDEX ix_oauth_states_expires
    ON google_integration.oauth_states (expires_at);
```

## Encrypted Token Format

AES-256-GCM with per-token nonce:

```
base64(nonce[12 bytes] + ciphertext[N bytes] + tag[16 bytes])
```

- Key: 256-bit from `GoogleIntegration:EncryptionKey` config
- Nonce: 96-bit random per encryption (never reused)
- Tag: 128-bit authentication tag (prevents tampering)

## EF Core Configuration

File: `Features/GoogleIntegration/Data/GoogleAccountLinkConfiguration.cs`

```csharp
public class GoogleAccountLinkConfiguration : IEntityTypeConfiguration<GoogleAccountLink>
{
    public void Configure(EntityTypeBuilder<GoogleAccountLink> builder)
    {
        builder.ToTable("google_account_links", "google_integration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => GoogleAccountLinkId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(e => e.GoogleAccountId)
            .HasConversion(id => id.Value, value => GoogleAccountId.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.GoogleEmail)
            .HasConversion(e => e.Value, value => Email.From(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(e => e.EncryptedAccessToken)
            .HasConversion(t => t.Value, value => EncryptedToken.From(value))
            .IsRequired();

        builder.Property(e => e.EncryptedRefreshToken)
            .HasConversion(t => t.Value, value => EncryptedToken.From(value))
            .IsRequired();

        builder.Property(e => e.GrantedScopes)
            .HasConversion(s => s.Value, value => GoogleScopes.From(value))
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(s => s.Value, value => GoogleLinkStatus.From(value))
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(GoogleLinkStatus.From("Active"));

        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.GoogleAccountId).IsUnique();
        builder.HasIndex(e => e.Status);

        builder.Ignore(e => e.DomainEvents);
    }
}
```
