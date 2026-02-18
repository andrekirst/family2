using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Zero-knowledge encrypted note. The server stores only ciphertext —
/// encryption/decryption happens exclusively on the client.
/// Title and content are base64-encoded AES-256-GCM ciphertext.
/// </summary>
public sealed class SecureNote : AggregateRoot<SecureNoteId>
{
#pragma warning disable CS8618
    private SecureNote() { }
#pragma warning restore CS8618

    public static SecureNote Create(
        FamilyId familyId,
        UserId userId,
        NoteCategory category,
        string encryptedTitle,
        string encryptedContent,
        string iv,
        string salt,
        string sentinel)
    {
        var note = new SecureNote
        {
            Id = SecureNoteId.New(),
            FamilyId = familyId,
            UserId = userId,
            Category = category,
            EncryptedTitle = encryptedTitle,
            EncryptedContent = encryptedContent,
            Iv = iv,
            Salt = salt,
            Sentinel = sentinel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        note.RaiseDomainEvent(new SecureNoteCreatedEvent(
            note.Id, category, familyId));

        return note;
    }

    public void Update(
        NoteCategory category,
        string encryptedTitle,
        string encryptedContent,
        string iv)
    {
        Category = category;
        EncryptedTitle = encryptedTitle;
        EncryptedContent = encryptedContent;
        Iv = iv;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        RaiseDomainEvent(new SecureNoteDeletedEvent(Id, FamilyId));
    }

    public FamilyId FamilyId { get; private set; }
    public UserId UserId { get; private set; }
    public NoteCategory Category { get; private set; }
    public string EncryptedTitle { get; private set; }
    public string EncryptedContent { get; private set; }
    public string Iv { get; private set; }
    public string Salt { get; private set; }
    public string Sentinel { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
}
