import { Stack } from "expo-router";
import { NotificationRouter } from "../notifications/NotificationRouter";
import { AuthProvider } from "../state/AuthContext";
import { CommunityProvider } from "../state/CommunityContext";
import { colors } from "../ui/theme";

export default function RootLayout() {
  return (
    <AuthProvider>
      <CommunityProvider>
        <NotificationRouter />
        <Stack
          screenOptions={{
            headerTintColor: colors.text,
            headerStyle: { backgroundColor: colors.card },
            contentStyle: { backgroundColor: colors.background },
          }}
        >
          <Stack.Screen name="index" options={{ headerShown: false }} />
          <Stack.Screen name="(auth)" options={{ headerShown: false }} />
          <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        </Stack>
      </CommunityProvider>
    </AuthProvider>
  );
}
