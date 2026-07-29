import { api } from "./client";
import type { ChatDetailsDto, ChatSummaryDto, CursorPage, MessageDto } from "./types";

function cursorQuery(name: "before" | "after", cursor: string | null, limit = 20) {
  const params = new URLSearchParams({ limit: String(limit) });
  if (cursor) params.set(name, cursor);
  return params.toString();
}

export const chatApi = {
  openDirect: (communityId: string, targetMembershipId: string) =>
    api.post<ChatDetailsDto>(`/api/communities/${communityId}/chats/direct`, {
      targetMembershipId,
    }),
  openTemporary: (communityId: string, issuerMembershipId: string) =>
    api.post<ChatDetailsDto>(`/api/communities/${communityId}/chats/temporary`, {
      issuerMembershipId,
    }),
  get: (communityId: string, chatId: string) =>
    api.get<ChatDetailsDto>(`/api/communities/${communityId}/chats/${chatId}`),
  getDirectPage: (communityId: string, before: string | null) =>
    api.get<CursorPage<ChatSummaryDto>>(
      `/api/communities/${communityId}/chats/direct?${cursorQuery("before", before)}`
    ),
  getHelpRequestPage: (communityId: string, before: string | null) =>
    api.get<CursorPage<ChatSummaryDto>>(
      `/api/communities/${communityId}/chats/help-requests?${cursorQuery("before", before)}`
    ),
  getTemporaryPage: (communityId: string, before: string | null) =>
    api.get<CursorPage<ChatSummaryDto>>(
      `/api/communities/${communityId}/chats/temporary?${cursorQuery("before", before)}`
    ),
};

export const helpChatApi = {
  respond: (communityId: string, helpRequestId: string) =>
    api.post<ChatDetailsDto>(`/api/communities/${communityId}/chats/help-requests/${helpRequestId}`),
  concludeWithReward: (communityId: string, chatId: string, stars: number) =>
    api.post<ChatDetailsDto>(`/api/communities/${communityId}/chats/${chatId}/conclude-with-reward`, {
      stars,
    }),
  concludeWithoutReward: (communityId: string, chatId: string) =>
    api.post<ChatDetailsDto>(
      `/api/communities/${communityId}/chats/${chatId}/conclude-without-reward`
    ),
  resign: (communityId: string, chatId: string) =>
    api.post<ChatDetailsDto>(`/api/communities/${communityId}/chats/${chatId}/resign`),
};

export const messageApi = {
  sendText: (communityId: string, chatId: string, text: string) =>
    api.post<MessageDto>(`/api/communities/${communityId}/chats/${chatId}/messages`, { text }),
  sendVoice: (communityId: string, chatId: string, audioUrl: string, durationSeconds: number) =>
    api.post<MessageDto>(`/api/communities/${communityId}/chats/${chatId}/messages/voice`, {
      audioUrl,
      durationSeconds,
    }),
  markRead: (communityId: string, chatId: string) =>
    api.post<void>(`/api/communities/${communityId}/chats/${chatId}/messages/read`),
  getPage: (communityId: string, chatId: string, after: string | null) =>
    api.get<CursorPage<MessageDto>>(
      `/api/communities/${communityId}/chats/${chatId}/messages?${cursorQuery("after", after, 50)}`
    ),
};
