import { api } from "./client";
import type {
  CertificationChallengeDto,
  CertificationResultDto,
  CommunityDataRequest,
  CommunityDetailsDto,
  CommunityQrDto,
  DocumentDto,
  JoinedCommunityDto,
  MemberProfileDto,
  MemberSummaryDto,
  MyCommunityDto,
  PagedResult,
  UnitNumberDto,
} from "./types";

export const communityApi = {
  create: (request: CommunityDataRequest) => api.post<CommunityDetailsDto>("/api/communities", request),
  getMine: () => api.get<MyCommunityDto[]>("/api/communities/mine"),
  get: (communityId: string) => api.get<CommunityDetailsDto>(`/api/communities/${communityId}`),
  update: (communityId: string, request: CommunityDataRequest) =>
    api.put<CommunityDetailsDto>(`/api/communities/${communityId}`, request),
  getQr: (communityId: string) => api.get<CommunityQrDto>(`/api/communities/${communityId}/qr`),
  join: (qrToken: string) => api.post<JoinedCommunityDto>("/api/communities/join", { qrToken }),
  leave: (communityId: string) => api.post<void>(`/api/communities/${communityId}/leave`),
};

export const memberApi = {
  getMine: (communityId: string) =>
    api.get<MemberProfileDto>(`/api/communities/${communityId}/members/me`),
  setUnitNumber: (communityId: string, unitNumber: string | null) =>
    api.put<UnitNumberDto>(`/api/communities/${communityId}/members/me/unit-number`, { unitNumber }),
  getConfirmed: (communityId: string) =>
    api.get<MemberSummaryDto[]>(`/api/communities/${communityId}/members`),
  getIssuers: (communityId: string) =>
    api.get<MemberSummaryDto[]>(`/api/communities/${communityId}/members/issuers`),
  getUnconfirmed: (communityId: string) =>
    api.get<MemberSummaryDto[]>(`/api/communities/${communityId}/members/unconfirmed`),
  getManager: (communityId: string) =>
    api.get<MemberSummaryDto | null>(`/api/communities/${communityId}/members/manager`),
  get: (communityId: string, membershipId: string) =>
    api.get<MemberProfileDto>(`/api/communities/${communityId}/members/${membershipId}`),
  grantIssuer: (communityId: string, membershipId: string) =>
    api.post<void>(`/api/communities/${communityId}/members/${membershipId}/roles/issuer`),
  getRanking: (communityId: string) =>
    api.get<MemberSummaryDto[]>(`/api/communities/${communityId}/ranking`),
};

export const certificationApi = {
  createChallenge: (communityId: string) =>
    api.post<CertificationChallengeDto>(`/api/communities/${communityId}/certification-challenges`),
  cancelChallenge: (communityId: string, challengeId: string) =>
    api.delete<void>(`/api/communities/${communityId}/certification-challenges/${challengeId}`),
  confirm: (token: string) =>
    api.post<CertificationResultDto>("/api/communities/certification-challenges/confirm", { token }),
};

export const documentApi = {
  add: (communityId: string, name: string, key: string) =>
    api.post<DocumentDto>(`/api/communities/${communityId}/documents`, { name, key }),
  getPaged: (communityId: string, page: number, pageSize: number) =>
    api.get<PagedResult<DocumentDto>>(
      `/api/communities/${communityId}/documents?page=${page}&pageSize=${pageSize}`
    ),
  remove: (communityId: string, documentId: string) =>
    api.delete<void>(`/api/communities/${communityId}/documents/${documentId}`),
};
