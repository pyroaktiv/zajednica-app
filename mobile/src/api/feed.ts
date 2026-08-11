import { api } from "./client";
import type {
  CommentDto,
  CursorPage,
  IntentDetailsDto,
  IntentSummaryDto,
  IntentVoterDto,
  PostDto,
} from "./types";

function cursorQuery(name: "before" | "after", cursor: string | null, limit = 20) {
  const params = new URLSearchParams({ limit: String(limit) });
  if (cursor) params.set(name, cursor);
  return params.toString();
}

export const postApi = {
  createGeneral: (communityId: string, text: string, kind: string, imageUrls: string[]) =>
    api.post<PostDto>(`/api/communities/${communityId}/posts`, { text, kind, imageUrls }),
  createHelpRequest: (communityId: string, text: string, imageUrls: string[]) =>
    api.post<PostDto>(`/api/communities/${communityId}/posts/help-requests`, { text, imageUrls }),
  closeHelpRequest: (communityId: string, postId: string) =>
    api.post<PostDto>(`/api/communities/${communityId}/posts/${postId}/close`),
  get: (communityId: string, postId: string) =>
    api.get<PostDto>(`/api/communities/${communityId}/posts/${postId}`),
  getPage: (communityId: string, before: string | null) =>
    api.get<CursorPage<PostDto>>(
      `/api/communities/${communityId}/posts?${cursorQuery("before", before)}`
    ),
};

export const commentApi = {
  add: (communityId: string, postId: string, text: string) =>
    api.post<CommentDto>(`/api/communities/${communityId}/posts/${postId}/comments`, { text }),
  reply: (communityId: string, postId: string, commentId: string, text: string) =>
    api.post<CommentDto>(
      `/api/communities/${communityId}/posts/${postId}/comments/${commentId}/replies`,
      { text }
    ),
  getRoots: (communityId: string, postId: string, after: string | null) =>
    api.get<CursorPage<CommentDto>>(
      `/api/communities/${communityId}/posts/${postId}/comments?${cursorQuery("after", after)}`
    ),
  getReplies: (communityId: string, postId: string, commentId: string, after: string | null) =>
    api.get<CursorPage<CommentDto>>(
      `/api/communities/${communityId}/posts/${postId}/comments/${commentId}/replies?${cursorQuery("after", after, 3)}`
    ),
};

export const intentApi = {
  openBan: (communityId: string, targetMembershipId: string, text: string) =>
    api.post<IntentDetailsDto>(`/api/communities/${communityId}/intents/ban`, {
      targetMembershipId,
      text,
    }),
  openManagerElection: (communityId: string, targetMembershipId: string, text: string) =>
    api.post<IntentDetailsDto>(`/api/communities/${communityId}/intents/manager-election`, {
      targetMembershipId,
      text,
    }),
  openMute: (communityId: string, targetMembershipId: string, text: string) =>
    api.post<IntentDetailsDto>(`/api/communities/${communityId}/intents/mute`, {
      targetMembershipId,
      text,
    }),
  vote: (communityId: string, intentId: string, value: boolean) =>
    api.post<IntentDetailsDto>(`/api/communities/${communityId}/intents/${intentId}/votes`, {
      value,
    }),
  get: (communityId: string, intentId: string) =>
    api.get<IntentDetailsDto>(`/api/communities/${communityId}/intents/${intentId}`),
  getVotes: (communityId: string, intentId: string) =>
    api.get<IntentVoterDto[]>(`/api/communities/${communityId}/intents/${intentId}/votes`),
  getPage: (communityId: string, before: string | null) =>
    api.get<CursorPage<IntentSummaryDto>>(
      `/api/communities/${communityId}/intents?${cursorQuery("before", before)}`
    ),
};
