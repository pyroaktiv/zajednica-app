import { router, Stack } from "expo-router";
import { useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import QRCode from "react-native-qrcode-svg";
import { communityApi } from "../api/community";
import type { CommunityQrDto } from "../api/types";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText, Loading } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

export default function CommunityQr() {
  const { activeCommunityId } = useCommunity();
  const [qr, setQr] = useState<CommunityQrDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!activeCommunityId) return;
    communityApi.getQr(activeCommunityId).then(setQr).catch((e) => setError(e.message));
  }, [activeCommunityId]);

  return (
    <View style={styles.container}>
      <Stack.Screen options={{ headerShown: false }} />
      {error && <ErrorText error={error} />}
      {!qr && !error && <Loading />}
      {qr && (
        <>
          <Text style={styles.title}>{qr.name}</Text>
          <Text style={styles.subtitle}>QR kod za pristup zajednici</Text>
          <View style={styles.qrBox}>
            <QRCode value={qr.qrToken} size={280} />
          </View>
          <Text style={styles.hint}>
            Odštampaj i zalepi na ulazna vrata zgrade. Novi stanari ga skeniraju iz aplikacije.
          </Text>
        </>
      )}
      <Button title="Zatvori" variant="secondary" onPress={() => router.back()} style={{ marginTop: spacing.xl, alignSelf: "stretch" }} />
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
    marginTop: spacing.xs,
    marginBottom: spacing.xl,
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
    textAlign: "center",
    marginTop: spacing.xl,
  },
});
