import { Link } from "expo-router";
import { useState } from "react";
import { KeyboardAvoidingView, Platform, Text, View } from "react-native";
import { useAuth } from "../../state/AuthContext";
import { Button, ErrorText, Field, Screen } from "../../ui/Basics";
import { colors, spacing } from "../../ui/theme";

export default function Login() {
  const { login } = useAuth();
  const [usernameOrEmail, setUsernameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    setError(null);
    setBusy(true);
    try {
      await login(usernameOrEmail.trim(), password);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Screen style={{ justifyContent: "center" }}>
      <KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : undefined}>
        <Text style={{ fontSize: 28, fontWeight: "800", color: colors.text, marginBottom: spacing.xl }}>
          zajednica.app
        </Text>
        <Field
          label="Korisničko ime ili mejl"
          value={usernameOrEmail}
          onChangeText={setUsernameOrEmail}
          autoCapitalize="none"
        />
        <Field label="Lozinka" value={password} onChangeText={setPassword} secureTextEntry />
        <ErrorText error={error} />
        <Button title="Prijavi se" onPress={submit} loading={busy} />
        <View style={{ flexDirection: "row", justifyContent: "center", marginTop: spacing.l, gap: spacing.l }}>
          <Link href="/register" style={{ color: colors.primary }}>
            Registracija
          </Link>
          <Link href="/verify" style={{ color: colors.primary }}>
            Aktivacija naloga
          </Link>
        </View>
      </KeyboardAvoidingView>
    </Screen>
  );
}
