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

function Row({ label, value, valueColor }: { label: string; value: string; valueColor?: string }) {
  return (
    <View style={{ flexDirection: "row", justifyContent: "space-between", marginBottom: spacing.s }}>
      <Text style={{ color: colors.muted }}>{label}</Text>
      <Text style={{ color: valueColor ?? colors.text, fontWeight: valueColor ? "700" : "500" }}>{value}</Text>
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
      const details = await intentApi.get(activeCommunityId, intentId);
      setIntent(details);
      setVoters(details.areVotesPublic ? await intentApi.getVotes(activeCommunityId, intentId) : []);
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
      const updated = await intentApi.vote(activeCommunityId, intent.id, value);
      setIntent(updated);
      if (updated.areVotesPublic) setVoters(await intentApi.getVotes(activeCommunityId, intent.id));
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
          <Text style={{ fontSize: 18, fontWeight: "800", color: colors.text, marginBottom: spacing.l }}>
            {intentKindLabel(intent.kind)}: {intent.targetUsername ?? "?"}
          </Text>
          <SectionTitle>Obrazloženje</SectionTitle>
          <Text style={{ color: colors.text, lineHeight: 22 }}>{intent.text}</Text>
        </Card>

        <Card>
          <View style={{ alignItems: "center", marginBottom: spacing.m }}>
            <Text
              style={{
                backgroundColor: open ? "#e8effc" : colors.background,
                color: open ? colors.primaryDark : colors.muted,
                fontWeight: "700",
                fontSize: 15,
                borderRadius: 6,
                paddingHorizontal: 8,
                paddingVertical: 3,
                overflow: "hidden",
              }}
            >
              {intentStatusLabel(intent.status).toUpperCase()}
            </Text>
          </View>
          <Row label="Inicijator" value={intent.authorUsername ?? "?"} />
          <Row label="Otvorena" value={formatDateTime(intent.dateCreated)} />
          <Row label="Rok" value={formatDateTime(intent.deadline)} />
          {intent.dateOfClosure && <Row label="Zaključena" value={formatDateTime(intent.dateOfClosure)} />}
          <Row label="Za" value={String(intent.votesFor)} valueColor={colors.success} />
          <Row label="Protiv" value={String(intent.votesAgainst)} valueColor={colors.danger} />
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
                variant="success"
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
          <SectionTitle>Glasovi</SectionTitle>
          {!intent.areVotesPublic && (
            <Text style={{ color: colors.muted }}>Glasovi za ovu vrstu namere nisu javni.</Text>
          )}
          {intent.areVotesPublic && voters.length === 0 && (
            <Text style={{ color: colors.muted }}>Još niko nije glasao.</Text>
          )}
          {intent.areVotesPublic && voters.length > 0 && (
            <View style={{ flexDirection: "row", gap: spacing.m }}>
              <View style={{ flex: 1 }}>
                <Text style={{ color: colors.success, fontWeight: "700", marginBottom: spacing.xs }}>ZA</Text>
                {voters.filter((v) => v.inFavor).map((voter) => (
                  <Text key={voter.membershipId} style={{ color: colors.text, paddingVertical: spacing.xs }}>
                    {voter.username ?? "?"}
                  </Text>
                ))}
              </View>
              <View style={{ flex: 1 }}>
                <Text style={{ color: colors.danger, fontWeight: "700", marginBottom: spacing.xs }}>PROTIV</Text>
                {voters.filter((v) => !v.inFavor).map((voter) => (
                  <Text key={voter.membershipId} style={{ color: colors.text, paddingVertical: spacing.xs }}>
                    {voter.username ?? "?"}
                  </Text>
                ))}
              </View>
            </View>
          )}
        </Card>
      </ScrollView>
    </Screen>
  );
}
