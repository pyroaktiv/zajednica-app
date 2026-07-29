import { Stack, useLocalSearchParams } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import { Alert, ScrollView, Text, View } from "react-native";
import { intentApi } from "../../api/feed";
import type { IntentDetailsDto, IntentVoterDto } from "../../api/types";
import { useChannel } from "../../realtime/connection";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, ErrorText, Loading, Screen, SectionTitle } from "../../ui/Basics";
import { formatDateTime, intentKindLabel, intentStatusLabel } from "../../ui/labels";
import { colors, spacing } from "../../ui/theme";

function Row({ label, value }: { label: string; value: string }) {
  return (
    <View style={{ flexDirection: "row", justifyContent: "space-between", marginBottom: spacing.s }}>
      <Text style={{ color: colors.muted }}>{label}</Text>
      <Text style={{ color: colors.text, fontWeight: "500" }}>{value}</Text>
    </View>
  );
}

export default function IntentDetails() {
  const { intentId } = useLocalSearchParams<{ intentId: string }>();
  const { activeCommunityId } = useCommunity();
  const [intent, setIntent] = useState<IntentDetailsDto | null>(null);
  const [voters, setVoters] = useState<IntentVoterDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    if (!activeCommunityId || !intentId) return;
    try {
      const [details, votes] = await Promise.all([
        intentApi.get(activeCommunityId, intentId),
        intentApi.getVotes(activeCommunityId, intentId),
      ]);
      setIntent(details);
      setVoters(votes);
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId, intentId]);

  useEffect(() => {
    load();
  }, [load]);

  useChannel(intentId ? `intent:${intentId}` : null, {
    "intent.updated": load,
  });

  if (error && !intent) {
    return (
      <Screen>
        <Stack.Screen options={{ title: "Namera" }} />
        <ErrorText error={error} />
      </Screen>
    );
  }
  if (!intent) return <Loading />;

  const vote = async (value: boolean) => {
    if (!activeCommunityId) return;
    setBusy(true);
    try {
      setIntent(await intentApi.vote(activeCommunityId, intent.id, value));
      setVoters(await intentApi.getVotes(activeCommunityId, intent.id));
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const open = intent.status === "Open";
  const quorumTarget = Math.floor(intent.eligibleVoterCount / 2) + 1;

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: "Namera" }} />
      <ScrollView contentContainerStyle={{ padding: spacing.l }}>
        <Card>
          <Text style={{ fontSize: 18, fontWeight: "800", color: colors.text }}>
            {intentKindLabel(intent.kind)}: {intent.targetUsername ?? "?"}
          </Text>
          <Text
            style={{
              color: open ? colors.primary : colors.muted,
              fontWeight: "700",
              marginTop: spacing.xs,
              marginBottom: spacing.m,
            }}
          >
            {intentStatusLabel(intent.status).toUpperCase()}
          </Text>
          <Text style={{ color: colors.text, lineHeight: 22 }}>{intent.text}</Text>
        </Card>

        <Card>
          <SectionTitle>Glasanje</SectionTitle>
          <Row label="Pokrenuo" value={intent.authorUsername ?? "?"} />
          <Row label="Otvorena" value={formatDateTime(intent.dateCreated)} />
          <Row label="Rok" value={formatDateTime(intent.deadline)} />
          {intent.dateOfClosure && <Row label="Zaključena" value={formatDateTime(intent.dateOfClosure)} />}
          <Row label="Za" value={String(intent.votesFor)} />
          <Row label="Protiv" value={String(intent.votesAgainst)} />
          <Row
            label="Kvorum"
            value={`${intent.votesFor + intent.votesAgainst}/${quorumTarget} ${intent.quorumReached ? "✔" : ""}`}
          />
          {intent.myVote !== null && (
            <Row label="Moj glas" value={intent.myVote ? "ZA" : "PROTIV"} />
          )}
          {open && intent.myVote === null && (
            <View style={{ flexDirection: "row", gap: spacing.m, marginTop: spacing.s }}>
              <Button
                title="Glasaj ZA"
                onPress={() => vote(true)}
                loading={busy}
                style={{ flex: 1 }}
              />
              <Button
                title="Glasaj PROTIV"
                variant="danger"
                onPress={() => vote(false)}
                loading={busy}
                style={{ flex: 1 }}
              />
            </View>
          )}
        </Card>

        <Card>
          <SectionTitle>Glasači</SectionTitle>
          {voters.length === 0 && <Text style={{ color: colors.muted }}>Još niko nije glasao.</Text>}
          {voters.map((voter) => (
            <View
              key={voter.membershipId}
              style={{ flexDirection: "row", justifyContent: "space-between", paddingVertical: spacing.xs }}
            >
              <Text style={{ color: colors.text }}>{voter.username ?? "?"}</Text>
              <Text style={{ color: voter.inFavor ? colors.success : colors.danger, fontWeight: "600" }}>
                {voter.inFavor ? "ZA" : "PROTIV"}
              </Text>
            </View>
          ))}
        </Card>
      </ScrollView>
    </Screen>
  );
}
