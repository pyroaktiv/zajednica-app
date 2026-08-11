import { router, Stack } from "expo-router";
import { useEffect, useState } from "react";
import { communityApi } from "../api/community";
import type { CommunityDetailsDto } from "../api/types";
import { useCommunity } from "../state/CommunityContext";
import { CommunityForm } from "../ui/CommunityForm";
import { ErrorText, Loading, Screen } from "../ui/Basics";

export default function CommunityEdit() {
  const { activeCommunityId, refresh } = useCommunity();
  const [community, setCommunity] = useState<CommunityDetailsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!activeCommunityId) return;
    communityApi.get(activeCommunityId).then(setCommunity).catch((e) => setError(e.message));
  }, [activeCommunityId]);

  if (error) {
    return (
      <Screen>
        <ErrorText error={error} />
      </Screen>
    );
  }
  if (!community || !activeCommunityId) return <Loading />;

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: "Podaci o zajednici" }} />
      <CommunityForm
        initial={community}
        submitTitle="Sačuvaj izmene"
        onSubmit={async (request) => {
          await communityApi.update(activeCommunityId, request);
          await refresh();
          router.back();
        }}
      />
    </Screen>
  );
}
