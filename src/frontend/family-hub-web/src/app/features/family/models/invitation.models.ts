export type FamilyRole = 'Owner' | 'Admin' | 'Member';
export type InvitationStatus = 'Pending' | 'Accepted' | 'Declined' | 'Revoked' | 'Expired';

export interface InvitationDto {
  id: string;
  familyId: string;
  familyName: string;
  invitedByName: string;
  inviteeEmail: string;
  role: FamilyRole;
  status: InvitationStatus;
  createdAt: string;
  expiresAt: string;
}

export interface FamilyMemberDto {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  role: FamilyRole;
  joinedAt: string;
  isActive: boolean;
}

export interface AcceptInvitationResult {
  familyId: string;
  familyMemberId: string;
  success: boolean;
}

export interface SendInvitationInput {
  email: string;
  role: string;
}

export interface AcceptInvitationInput {
  token: string;
}
