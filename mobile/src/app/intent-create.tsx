import { router, Stack, useLocalSearchParams } from "expo-router";
import { useState } from "react";
import { Pressable, ScrollView, Text, TextInput, View } from "react-native";
import { intentApi } from "../api/feed";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText, Screen, SectionTitle } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

type Kind = "ban" | "managerElection";

export default function IntentCreate() {
  const { targetMembershipId, username } = useLocalSearchParams<{
    targetMembershipId: string;
    username: string;
  }>();
  const { activeCommunityId } = useCommunity();
  const [kind, setKind] = useState<Kind>("managerElection");
  const [text, setText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    if (!activeCommunityId || !targetMembershipId) return;
    setError(null);
    setBusy(true);
    try {
      const opened =
        kind === "ban"
          ? await intentApi.openBan(activeCommunityId, targetMembershipId, text.trim())
          : await intentApi.openManagerElection(activeCommunityId, targetMembershipId, text.trim());
      router.replace({ pathname: "/intent/[intentId]", params: { intentId: opened.id } });
    } catch (e: any) {
      setError(e.message);
      setBusy(false);
    }
  };

  const option = (value: Kind, label: string) => (
    <Pressable
      onPress={() => setKind(value)}
      style={{
        flex: 1,
        padding: spacing.m,
        borderRadius: 8,
        borderWidth: 1,
        alignItems: "center",
        borderColor: kind === value ? colors.primary : colors.border,
        backgroundColor: kind === value ? colors.primary : colors.card,
      }}
    >
      <Text style={{ color: kind === value ? "#fff" : colors.text, fontWeight: "600", textAlign: "center" }}>
        {label}
      </Text>
    </Pressable>
  );

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: "Nova namera" }} />
      <ScrollView contentContainerStyle={{ padding: spacing.l }}>
        <Text style={{ color: colors.muted, marginBottom: spacing.m }}>
          Namera se odnosi na korisnika{" "}
          <Text style={{ fontWeight: "700", color: colors.text }}>{username}</Text>. O nameri glasaju
          svi potvrđeni članovi zajednice.
        </Text>
        <SectionTitle>Akcija</SectionTitle>
        <View style={{ flexDirection: "row", gap: spacing.m, marginBottom: spacing.l }}>
          {option("managerElection", "Postavljanje za upravnika")}
          {option("ban", "Izbacivanje iz zajednice")}
        </View>
        <SectionTitle>Obrazloženje</SectionTitle>
        <TextInput
          multiline
          value={text}
          onChangeText={setText}
          placeholder="Zašto pokrećeš ovu nameru?"
          placeholderTextColor={colors.muted}
          style={{
            backgroundColor: colors.card,
            borderWidth: 1,
            borderColor: colors.border,
            borderRadius: 8,
            padding: spacing.m,
            minHeight: 120,
            textAlignVertical: "top",
            color: colors.text,
            marginBottom: spacing.m,
          }}
        />
        <ErrorText error={error} />
        <Button title="Otvori nameru" onPress={submit} loading={busy} />
      </ScrollView>
    </Screen>
  );
}
