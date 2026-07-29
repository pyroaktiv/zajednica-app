import { router, Stack } from "expo-router";
import { useEffect, useRef, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import QRCode from "react-native-qrcode-svg";
import { certificationApi } from "../api/community";
import type { CertificationChallengeDto } from "../api/types";
import { useChannel } from "../realtime/connection";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText, Loading } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

export default function CertifyShow() {
  const { activeCommunityId } = useCommunity();
  const [challenge, setChallenge] = useState<CertificationChallengeDto | null>(null);
  const [confirmed, setConfirmed] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const challengeRef = useRef<CertificationChallengeDto | null>(null);

  useEffect(() => {
    if (!activeCommunityId) return;
    certificationApi
      .createChallenge(activeCommunityId)
      .then((created) => {
        setChallenge(created);
        challengeRef.current = created;
      })
      .catch((e) => setError(e.message));
    return () => {
      const pending = challengeRef.current;
      if (pending) certificationApi.cancelChallenge(activeCommunityId, pending.challengeId).catch(() => {});
    };
  }, [activeCommunityId]);

  useChannel(activeCommunityId ? `community:${activeCommunityId}` : null, {
    "certification.confirmed": (payload: { challengeId: string }) => {
      if (payload?.challengeId === challengeRef.current?.challengeId) {
        challengeRef.current = null;
        setConfirmed(true);
      }
    },
  });

  return (
    <View style={styles.container}>
      <Stack.Screen options={{ headerShown: false }} />
      <ErrorText error={error} />
      {!challenge && !error && <Loading />}
      {challenge && !confirmed && (
        <>
          <Text style={styles.title}>Izdavanje potvrde</Text>
          <Text style={styles.subtitle}>
            Neka kandidat skenira ovaj QR kod svojim telefonom, iz aplikacije.
          </Text>
          <View style={styles.qrBox}>
            <QRCode value={challenge.token} size={280} />
          </View>
          <Text style={styles.hint}>
            Kod važi do {new Date(challenge.expiresAt).toLocaleTimeString("sr-RS")}.
          </Text>
        </>
      )}
      {confirmed && (
        <>
          <Text style={[styles.title, { color: colors.success }]}>Potvrda izdata ✔</Text>
          <Text style={styles.subtitle}>Kandidat je uspešno potvrđen.</Text>
        </>
      )}
      <Button
        title="Zatvori"
        variant="secondary"
        onPress={() => router.back()}
        style={{ marginTop: spacing.xl, alignSelf: "stretch" }}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.card,
    alignItems: "center",
    justifyContent: "center",
    padding: spacing.xl,
  },
  title: {
    fontSize: 24,
    fontWeight: "800",
    color: colors.text,
  },
  subtitle: {
    color: colors.muted,
    marginTop: spacing.s,
    marginBottom: spacing.xl,
    textAlign: "center",
  },
  qrBox: {
    padding: spacing.l,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    backgroundColor: "#fff",
  },
  hint: {
    color: colors.muted,
    marginTop: spacing.l,
  },
});
