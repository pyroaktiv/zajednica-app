import { Ionicons } from "@expo/vector-icons";
import { Redirect, Tabs } from "expo-router";
import { ColorValue } from "react-native";
import { useAuth } from "../../state/AuthContext";
import { useCommunity } from "../../state/CommunityContext";
import { Loading } from "../../ui/Basics";
import { colors } from "../../ui/theme";

type IconName = keyof typeof Ionicons.glyphMap;

export default function TabsLayout() {
  const { status: authStatus } = useAuth();
  const { loading, status } = useCommunity();

  if (authStatus === "loading" || loading) return <Loading />;
  if (authStatus === "signedOut") return <Redirect href="/login" />;

  const chatsEnabled = status !== "none";
  const fullAccess = status === "confirmed";

  const tab = (icon: IconName, title: string, enabled: boolean) => ({
    title,
    tabBarIcon: ({ color, size }: { color: ColorValue; size: number }) => (
      <Ionicons name={icon} color={enabled ? color : colors.border} size={size} />
    ),
    tabBarLabelStyle: enabled ? undefined : { color: colors.border },
  });

  const guard = (enabled: boolean) => ({
    tabPress: (e: { preventDefault: () => void }) => {
      if (!enabled) e.preventDefault();
    },
  });

  return (
    <Tabs
      screenOptions={{
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.muted,
        headerTintColor: colors.text,
        headerStyle: { backgroundColor: colors.card },
        sceneStyle: { backgroundColor: colors.background },
      }}
    >
      <Tabs.Screen
        name="home"
        options={{ ...tab("home", "Početna", fullAccess), headerTitle: "Početna" }}
        listeners={guard(fullAccess)}
      />
      <Tabs.Screen
        name="members"
        options={{ ...tab("people", "Članovi", fullAccess), headerTitle: "Članovi" }}
        listeners={guard(fullAccess)}
      />
      <Tabs.Screen
        name="feed"
        options={{ ...tab("newspaper", "Objave", fullAccess), headerTitle: "Objave" }}
        listeners={guard(fullAccess)}
      />
      <Tabs.Screen
        name="chats"
        options={{ ...tab("chatbubbles", "Četovi", chatsEnabled), headerTitle: "Četovi" }}
        listeners={guard(chatsEnabled)}
      />
      <Tabs.Screen
        name="profile"
        options={{ ...tab("person", "Profil", true), headerTitle: "Profil" }}
      />
    </Tabs>
  );
}
