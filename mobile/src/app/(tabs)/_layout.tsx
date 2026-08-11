import { Ionicons } from "@expo/vector-icons";
import { Redirect, router, Tabs } from "expo-router";
import { ColorValue, Pressable } from "react-native";
import { useAuth } from "../../state/AuthContext";
import { useCommunity } from "../../state/CommunityContext";
import { Loading } from "../../ui/Basics";
import { colors } from "../../ui/theme";

type IconName = keyof typeof Ionicons.glyphMap;

export default function TabsLayout() {
  const { status: authStatus } = useAuth();
  const { loading } = useCommunity();

  if (authStatus === "loading" || loading) return <Loading />;
  if (authStatus === "signedOut") return <Redirect href="/login" />;

  const tab = (icon: IconName, title: string) => ({
    title,
    headerTitle: title,
    tabBarIcon: ({ color, size }: { color: ColorValue; size: number }) => (
      <Ionicons name={icon} color={color} size={size} />
    ),
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
        options={{
          ...tab("home", "Početna"),
          headerRight: () => (
            <Pressable
              onPress={() => router.push("/communities")}
              hitSlop={12}
              style={{ marginRight: 16 }}
            >
              <Ionicons name="swap-horizontal-outline" size={22} color={colors.primary} />
            </Pressable>
          ),
        }}
      />
      <Tabs.Screen name="members" options={tab("people", "Članovi")} />
      <Tabs.Screen name="feed" options={tab("newspaper", "Objave")} />
      <Tabs.Screen name="chats" options={tab("chatbubbles", "Četovi")} />
      <Tabs.Screen name="profile" options={tab("person", "Profil")} />
    </Tabs>
  );
}
