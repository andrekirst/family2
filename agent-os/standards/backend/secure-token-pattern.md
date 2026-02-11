# Secure Token Pattern

Generate secure tokens where the plaintext is only ever in transit (domain event), and only the SHA256 hash is persisted in the database.

## Token Generation

```csharp
var randomBytes = new byte[48];
RandomNumberGenerator.Fill(randomBytes);
var plaintextToken = Convert.ToBase64String(randomBytes)
    .Replace("+", "-").Replace("/", "_").TrimEnd('=');
var tokenHash = ComputeSha256Hash(plaintextToken);
```

## SHA256 Hashing

```csharp
public static string ComputeSha256Hash(string input)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexStringLower(bytes);
}
```

## Token Flow

1. **Handler generates** plaintext + hash
2. **Aggregate stores** hash only (via `InvitationToken` VO)
3. **Domain event carries** plaintext (for building email URL)
4. **Event handler** uses plaintext to build URL, then discards it
5. **Verification** hashes incoming token, looks up by hash in DB

## InvitationToken Value Object

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial struct InvitationToken
{
    private static Validation Validate(string value) =>
        value.Length == 64 && value.All(c => "0123456789abcdef".Contains(c))
            ? Validation.Ok
            : Validation.Invalid("Token hash must be 64 hex characters");
}
```

The VO only accepts the 64-char hex hash, never the plaintext.

## Security Properties

- Plaintext never stored in DB
- CSPRNG (RandomNumberGenerator) for entropy
- DB query by hash (timing-safe)
- URL-safe Base64 encoding
- 48 bytes = 384 bits of randomness

## Rules

- Always use `RandomNumberGenerator` (never `Random` or `Guid`)
- Always hash with SHA256 before storing
- Plaintext only exists in the domain event (transient)
- Token VO validates 64-char hex string (the hash)
- Use `Convert.ToHexStringLower()` for consistent lowercase hex
