import { router } from "expo-router";
import * as Notifications from "expo-notifications";
import { useEffect, useRef } from "react";
import { useCommunity } from "../state/CommunityContext";
import { useAuth } from "../state/AuthContext";

const routeBuilders: Record<string, (id: string) => string> = {
  chat: (id) => `/chat/${id}`,
  post: (id) => `/post/${id}`,
  intent: (id) => `/intent/${id}`,
};

function routeFor(kind?: string, id?: string): string | null {
  if (!kind || !id) return null;
  return routeBuilders[kind]?.(id) ?? null;
}

export function NotificationRouter() {
  const { status } = useAuth();
  const { loading, activeCommunityId, communities, setActiveCommunity } = useCommunity();
  const response = Notifications.useLastNotificationResponse();
  const handledId = useRef<string | null>(null);

  useEffect(() => {
    if (!response) return;

    const id = response.notification.request.identifier;
    if (handledId.current === id) return;

    const data = response.notification.request.content.data as { kind?: string; id?: string; communityId?: string };
    const route = routeFor(data.kind, data.id);
    if (!route) return;

    if (status !== "signedIn" || loading) return;

    if (data.communityId && data.communityId !== activeCommunityId && communities.some((c) => c.id === data.communityId)) {
      setActiveCommunity(data.communityId);
      return;
    }

    handledId.current = id;
    router.replace("/home");
    router.push(route as never);
  }, [response, status, loading, activeCommunityId, communities, setActiveCommunity]);

  return null;
}
