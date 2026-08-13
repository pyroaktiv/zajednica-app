export type AuthTokens = { accessToken: string; refreshToken: string };

export type RegisterAccountRequest = {
  username: string;
  email: string;
  password: string;
  firstName?: string | null;
  lastName?: string | null;
  phone?: string | null;
  contactEmail?: string | null;
};

export type ProfileDto = {
  username: string;
  firstName: string | null;
  lastName: string | null;
  phone: string | null;
  contactEmail: string | null;
  imageUrl: string | null;
};

export type UpdateProfileRequest = {
  firstName: string | null;
  lastName: string | null;
  phone: string | null;
  contactEmail: string | null;
};

export type AddressDto = {
  street: string;
  number: string;
  latitude: number | null;
  longitude: number | null;
};

export type CommunityDetailsDto = {
  id: string;
  name: string;
  address: AddressDto;
  registrationNumber: string | null;
  taxId: string | null;
  bankAccountNumber: string | null;
  dateCreated: string;
};

export type CommunityDataRequest = {
  name: string;
  address: AddressDto;
  registrationNumber: string | null;
  taxId: string | null;
  bankAccountNumber: string | null;
};

export type MyCommunityDto = {
  id: string;
  name: string;
  address: AddressDto;
  isConfirmed: boolean;
  roles: string[];
};

export type CommunityQrDto = { communityId: string; name: string; qrToken: string };

export type JoinedCommunityDto = {
  membershipId: string;
  communityId: string;
  communityName: string;
  isConfirmed: boolean;
};

export type MemberSummaryDto = {
  membershipId: string;
  accountId: string;
  username: string;
  firstName: string | null;
  lastName: string | null;
  imageUrl: string | null;
  isConfirmed: boolean;
  stars: number | null;
  roles: string[];
};

export type MemberProfileDto = {
  membershipId: string;
  accountId: string;
  username: string;
  imageUrl: string | null;
  firstName: string | null;
  lastName: string | null;
  phone: string | null;
  contactEmail: string | null;
  unitNumber: string | null;
  isConfirmed: boolean;
  stars: number | null;
  roles: string[];
  dateJoined: string;
  state: string;
  mutedUntil: string | null;
};

export type UnitNumberDto = { membershipId: string; unitNumber: string | null };

export type CertificationChallengeDto = {
  challengeId: string;
  token: string;
  expiresAt: string;
};

export type CertificationResultDto = {
  membershipId: string;
  communityId: string;
  certifiedAt: string;
};

export type DocumentDto = {
  id: string;
  name: string;
  contentUrl: string;
  postedByMembershipId: string;
  date: string;
};

export type PostDto = {
  id: string;
  type: string;
  kind: string | null;
  closed: boolean | null;
  authorMembershipId: string;
  authorUsername: string;
  authorImageUrl: string | null;
  text: string;
  imageUrls: string[];
  dateCreated: string;
};

export type CommentDto = {
  id: string;
  postId: string;
  parentCommentId: string | null;
  authorMembershipId: string;
  authorUsername: string;
  authorImageUrl: string | null;
  text: string;
  hasReplies: boolean;
  date: string;
};

export type IntentSummaryDto = {
  id: string;
  kind: string;
  status: string;
  authorMembershipId: string;
  targetMembershipId: string | null;
  targetUsername: string | null;
  text: string;
  dateCreated: string;
  deadline: string;
  eligibleVoterCount: number;
  votesFor: number;
  votesAgainst: number;
};

export type IntentDetailsDto = {
  id: string;
  kind: string;
  status: string;
  authorMembershipId: string;
  authorUsername: string | null;
  targetMembershipId: string | null;
  targetUsername: string | null;
  text: string;
  dateCreated: string;
  deadline: string;
  dateOfClosure: string | null;
  eligibleVoterCount: number;
  votesFor: number;
  votesAgainst: number;
  quorumReached: boolean;
  myVote: boolean | null;
  areVotesPublic: boolean;
};

export type IntentVoterDto = {
  membershipId: string;
  username: string | null;
  inFavor: boolean;
  votedAt: string;
};

export type ChatParticipantDto = {
  membershipId: string;
  username: string;
  role: string | null;
};

export type ChatDetailsDto = {
  id: string;
  type: string;
  participants: ChatParticipantDto[];
  canSend: boolean;
  helpRequestId: string | null;
  helpRequestPreview: string | null;
  status: string | null;
  awardedStars: number | null;
};

export type ChatSummaryDto = {
  id: string;
  participantUsernames: string[];
  helpRequestId: string | null;
  status: string | null;
  viewerRole: string | null;
  lastActivityAt: string;
  hasUnread: boolean;
};

export type MessageDto = {
  id: string;
  chatId: string;
  senderMembershipId: string;
  senderUsername: string;
  type: string;
  text: string | null;
  audioUrl: string | null;
  durationSeconds: number | null;
  date: string;
};

export type CursorPage<T> = { items: T[]; nextCursor: string | null };

export type PagedResult<T> = { results: T[]; totalCount: number };

export type UploadedFileDto = { key: string };

export const RoleNames = { Issuer: "Issuer", Manager: "Manager" } as const;
