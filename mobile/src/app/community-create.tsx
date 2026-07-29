import { router, Stack } from "expo-router";
import { communityApi } from "../api/community";
import { useCommunity } from "../state/CommunityContext";
import { CommunityForm } from "../ui/CommunityForm";
import { Screen } from "../ui/Basics";

export default function CommunityCreate() {
  const { setActiveCommunity } = useCommunity();

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: "Nova zajednica" }} />
      <CommunityForm
        submitTitle="Kreiraj zajednicu"
        onSubmit={async (request) => {
          const created = await communityApi.create(request);
          setActiveCommunity(created.id);
          router.dismissTo("/home");
        }}
      />
    </Screen>
  );
}
