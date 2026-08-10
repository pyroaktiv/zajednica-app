import { router } from "expo-router";
import * as Notifications from "expo-notifications";
import { useEffect, useRef } from "react";
import { useCommunity } from "../state/CommunityContext";
import { useAuth } from "../state/AuthContext";

export function NotificationRouter() {
  const { status } = useAuth();
  const { loading, activeCommunityId, communities, setActiveCommunity } = useCommunity();
  const response = Notifications.useLastNotificationResponse();
  const handledId = useRef<string | null>(null);

  useEffect(() => {
    if (!response) return;

    const id = response.notification.request.identifier;
    if (handledId.current === id) return;

    const data = response.notification.request.content.data as { route?: string; communityId?: string };
    if (!data?.route) return;

    if (status !== "signedIn" || loading) return;

    if (data.communityId && data.communityId !== activeCommunityId && communities.some((c) => c.id === data.communityId)) {
      setActiveCommunity(data.communityId);
      return;
    }

    handledId.current = id;
    router.replace("/home");
    router.push(data.route as never);
  }, [response, status, loading, activeCommunityId, communities, setActiveCommunity]);

  return null;
}
