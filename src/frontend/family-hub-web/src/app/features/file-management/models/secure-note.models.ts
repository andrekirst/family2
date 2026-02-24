export interface SecureNoteDto {
  id: string;
  familyId: string;
  userId: string;
  category: string;
  encryptedTitle: string;
  encryptedContent: string;
  iv: string;
  salt: string;
  sentinel: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSecureNoteInput {
  category: string;
  encryptedTitle: string;
  encryptedContent: string;
  iv: string;
  salt: string;
  sentinel: string;
}

export interface UpdateSecureNoteInput {
  noteId: string;
  category: string;
  encryptedTitle: string;
  encryptedContent: string;
  iv: string;
}

/** Decrypted note for UI display (never sent to server) */
export interface DecryptedNote {
  id: string;
  category: string;
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string;
}
