import { router } from "expo-router";
import { useState } from "react";
import { ScrollView, Text } from "react-native";
import { authApi } from "../../api/identity";
import { Button, ErrorText, Field, Screen } from "../../ui/Basics";
import { colors, spacing } from "../../ui/theme";

export default function Register() {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phone, setPhone] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    setError(null);
    setBusy(true);
    try {
      await authApi.register({
        username: username.trim(),
        email: email.trim(),
        password,
        firstName: firstName.trim() || null,
        lastName: lastName.trim() || null,
        phone: phone.trim() || null,
        contactEmail: contactEmail.trim() || null,
      });
      router.replace("/verify");
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Screen style={{ padding: 0 }}>
      <ScrollView contentContainerStyle={{ padding: spacing.l, paddingTop: spacing.xl * 2 }}>
        <Text style={{ fontSize: 24, fontWeight: "800", color: colors.text, marginBottom: spacing.xl }}>
          Registracija
        </Text>
        <Field label="Korisničko ime *" value={username} onChangeText={setUsername} autoCapitalize="none" />
        <Field
          label="Mejl adresa *"
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
        />
        <Field label="Lozinka *" value={password} onChangeText={setPassword} secureTextEntry />
        <Field label="Ime" value={firstName} onChangeText={setFirstName} />
        <Field label="Prezime" value={lastName} onChangeText={setLastName} />
        <Field label="Telefon" value={phone} onChangeText={setPhone} keyboardType="phone-pad" />
        <Field
          label="Kontakt mejl"
          value={contactEmail}
          onChangeText={setContactEmail}
          autoCapitalize="none"
          keyboardType="email-address"
        />
        <ErrorText error={error} />
        <Button title="Registruj se" onPress={submit} loading={busy} />
        <Button
          title="Nazad na prijavu"
          variant="secondary"
          onPress={() => router.back()}
          style={{ marginTop: spacing.m }}
        />
      </ScrollView>
    </Screen>
  );
}
