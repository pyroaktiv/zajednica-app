import { router } from "expo-router";
import { useState } from "react";
import { Text } from "react-native";
import { authApi } from "../../api/identity";
import { Button, ErrorText, Field, Screen } from "../../ui/Basics";
import { colors, spacing } from "../../ui/theme";

export default function Verify() {
  const [token, setToken] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    setError(null);
    setBusy(true);
    try {
      await authApi.verifyEmail(token.trim());
      router.replace("/login");
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Screen style={{ justifyContent: "center" }}>
      <Text style={{ fontSize: 24, fontWeight: "800", color: colors.text, marginBottom: spacing.m }}>
        Aktivacija naloga
      </Text>
      <Text style={{ color: colors.muted, marginBottom: spacing.l }}>
        Na mejl adresu je stigao aktivacioni kod. Unesi ga ovde da aktiviraš nalog.
      </Text>
      <Field label="Aktivacioni kod" value={token} onChangeText={setToken} autoCapitalize="none" />
      <ErrorText error={error} />
      <Button title="Aktiviraj" onPress={submit} loading={busy} />
      <Button
        title="Nazad na prijavu"
        variant="secondary"
        onPress={() => router.replace("/login")}
        style={{ marginTop: spacing.m }}
      />
    </Screen>
  );
}
