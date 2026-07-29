import { CameraView, useCameraPermissions } from "expo-camera";
import { router, Stack } from "expo-router";
import { useEffect, useRef, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { certificationApi } from "../api/community";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

export default function CertifyScan() {
  const [permission, requestPermission] = useCameraPermissions();
  const { refresh } = useCommunity();
  const [error, setError] = useState<string | null>(null);
  const scanned = useRef(false);

  useEffect(() => {
    if (permission && !permission.granted) requestPermission();
  }, [permission]);

  const onScanned = async ({ data }: { data: string }) => {
    if (scanned.current) return;
    scanned.current = true;
    try {
      await certificationApi.confirm(data);
      await refresh();
      router.dismissTo("/home");
    } catch (e: any) {
      setError(e.message);
    }
  };

  return (
    <View style={styles.container}>
      <Stack.Screen options={{ headerShown: false }} />
      {permission?.granted ? (
        <CameraView
          style={StyleSheet.absoluteFill}
          barcodeScannerSettings={{ barcodeTypes: ["qr"] }}
          onBarcodeScanned={onScanned}
        />
      ) : (
        <Text style={{ color: "#fff", textAlign: "center" }}>
          Potrebna je dozvola za korišćenje kamere.
        </Text>
      )}
      <View style={styles.overlay}>
        <Text style={styles.hint}>
          Skeniraj QR kod sa telefona izdavača potvrde
        </Text>
        {error && (
          <View style={{ backgroundColor: colors.card, borderRadius: 8, padding: spacing.m }}>
            <ErrorText error={error} />
            <Button
              title="Pokušaj ponovo"
              onPress={() => {
                setError(null);
                scanned.current = false;
              }}
            />
          </View>
        )}
        <Button
          title="Zatvori"
          variant="secondary"
          onPress={() => router.back()}
          style={{ marginTop: spacing.m }}
        />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#000",
    justifyContent: "center",
  },
  overlay: {
    position: "absolute",
    bottom: 40,
    left: spacing.l,
    right: spacing.l,
  },
  hint: {
    color: "#fff",
    textAlign: "center",
    marginBottom: spacing.m,
    fontSize: 16,
  },
});
