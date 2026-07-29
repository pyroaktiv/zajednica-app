import { Ionicons } from "@expo/vector-icons";
import { router, Stack } from "expo-router";
import { Alert, FlatList, Pressable, Text, View } from "react-native";
import { communityApi } from "../api/community";
import { useCommunity } from "../state/CommunityContext";
import { Button, Card, EmptyState, Screen } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

export default function Communities() {
  const { communities, activeCommunityId, setActiveCommunity } = useCommunity();

  const leave = (communityId: string, name: string) => {
    Alert.alert("Istupanje", `Da li sigurno želiš da istupiš iz zajednice "${name}"?`, [
      { text: "Odustani", style: "cancel" },
      {
        text: "Istupi",
        style: "destructive",
        onPress: async () => {
          try {
            await communityApi.leave(communityId);
            setActiveCommunity(null);
          } catch (e: any) {
            Alert.alert("Greška", e.message);
          }
        },
      },
    ]);
  };

  return (
    <Screen>
      <Stack.Screen options={{ title: "Moje zajednice" }} />
      <FlatList
        data={communities}
        keyExtractor={(c) => c.id}
        ListEmptyComponent={<EmptyState text="Nisi član nijedne stambene zajednice." />}
        renderItem={({ item }) => (
          <Pressable
            onPress={() => {
              setActiveCommunity(item.id);
              router.back();
            }}
          >
            <Card
              style={
                item.id === activeCommunityId
                  ? { borderColor: colors.primary, borderWidth: 2 }
                  : undefined
              }
            >
              <View style={{ flexDirection: "row", alignItems: "center" }}>
                <View style={{ flex: 1 }}>
                  <Text style={{ fontWeight: "700", fontSize: 16, color: colors.text }}>
                    {item.name}
                  </Text>
                  <Text style={{ color: colors.muted, marginTop: 2 }}>
                    {item.address.street} {item.address.number}
                  </Text>
                  <Text style={{ color: colors.muted, marginTop: 2, fontSize: 12 }}>
                    {item.isConfirmed ? "potvrđen član" : "nepotvrđen član"}
                    {item.roles.length > 0 ? ` · ${item.roles.join(", ")}` : ""}
                  </Text>
                </View>
                <Pressable onPress={() => leave(item.id, item.name)} hitSlop={12}>
                  <Ionicons name="exit-outline" size={22} color={colors.danger} />
                </Pressable>
              </View>
            </Card>
          </Pressable>
        )}
      />
      <Button
        title="Skeniraj QR kod za pristup"
        onPress={() => router.push("/join-scan")}
        style={{ marginBottom: spacing.m }}
      />
      <Button
        title="Kreiraj novu zajednicu"
        variant="secondary"
        onPress={() => router.push("/community-create")}
      />
    </Screen>
  );
}
