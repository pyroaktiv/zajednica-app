import AsyncStorage from "@react-native-async-storage/async-storage";
import { createContext, ReactNode, useCallback, useContext, useEffect, useState } from "react";
import { communityApi, memberApi } from "../api/community";
import type { MemberProfileDto, MyCommunityDto } from "../api/types";
import { RoleNames } from "../api/types";
import { useChannel } from "../realtime/connection";
import { useAuth } from "./AuthContext";

const ACTIVE_KEY = "community.active";

export type MembershipStatus = "none" | "unconfirmed" | "confirmed";

type CommunityContextValue = {
  loading: boolean;
  communities: MyCommunityDto[];
  activeCommunityId: string | null;
  me: MemberProfileDto | null;
  status: MembershipStatus;
  isIssuer: boolean;
  isManager: boolean;
  isMuted: boolean;
  mutedUntil: string | null;
  setActiveCommunity: (communityId: string | null) => void;
  refresh: () => Promise<void>;
};

const CommunityContext = createContext<CommunityContextValue>(null!);

export function CommunityProvider({ children }: { children: ReactNode }) {
  const { status: authStatus } = useAuth();
  const [loading, setLoading] = useState(true);
  const [communities, setCommunities] = useState<MyCommunityDto[]>([]);
  const [activeCommunityId, setActiveCommunityId] = useState<string | null>(null);
  const [me, setMe] = useState<MemberProfileDto | null>(null);

  const load = useCallback(async (preferredId: string | null) => {
    const mine = await communityApi.getMine();
    setCommunities(mine);
    const active = mine.find((c) => c.id === preferredId) ?? mine[0] ?? null;
    setActiveCommunityId(active?.id ?? null);
    setMe(active ? await memberApi.getMine(active.id) : null);
  }, []);

  useEffect(() => {
    if (authStatus !== "signedIn") {
      setCommunities([]);
      setActiveCommunityId(null);
      setMe(null);
      setLoading(authStatus === "loading");
      return;
    }
    setLoading(true);
    AsyncStorage.getItem(ACTIVE_KEY)
      .then((stored) => load(stored))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [authStatus, load]);

  const setActiveCommunity = (communityId: string | null) => {
    AsyncStorage.setItem(ACTIVE_KEY, communityId ?? "").catch(() => {});
    setLoading(true);
    load(communityId)
      .catch(() => {})
      .finally(() => setLoading(false));
  };

  const refresh = useCallback(
    () => load(activeCommunityId).catch(() => {}),
    [load, activeCommunityId]
  );

  useChannel(activeCommunityId ? `community:${activeCommunityId}` : null, {
    "membership.roles.changed": refresh,
    "membership.mute.changed": refresh,
    "certification.confirmed": refresh,
  });

  const status: MembershipStatus = !me ? "none" : me.isConfirmed ? "confirmed" : "unconfirmed";
  const mutedUntil = me?.mutedUntil ?? null;

  return (
    <CommunityContext.Provider
      value={{
        loading,
        communities,
        activeCommunityId,
        me,
        status,
        isIssuer: (me?.roles.some((r) => r === RoleNames.Issuer || r === RoleNames.Manager)) ?? false,
        isManager: me?.roles.includes(RoleNames.Manager) ?? false,
        isMuted: mutedUntil !== null && new Date(mutedUntil) > new Date(),
        mutedUntil,
        setActiveCommunity,
        refresh,
      }}
    >
      {children}
    </CommunityContext.Provider>
  );
}

export function useCommunity() {
  return useContext(CommunityContext);
}
