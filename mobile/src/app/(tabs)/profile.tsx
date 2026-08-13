import { Image } from "expo-image";
import { router, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { ScrollView, Text, View } from "react-native";
import { authorizedFile } from "../../api/client";
import { profileApi } from "../../api/identity";
import type { ProfileDto } from "../../api/types";
import { useAuth } from "../../state/AuthContext";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, ErrorText, Loading, Screen, SectionTitle } from "../../ui/Basics";
import { roleLabel } from "../../ui/MemberRow";
import { colors, spacing } from "../../ui/theme";

function Row({ label, value }: { label: string; value: string | null }) {
  return (
    <View style={{ flexDirection: "row", justifyContent: "space-between", marginBottom: spacing.s }}>
      <Text style={{ color: colors.muted }}>{label}</Text>
      <Text style={{ color: colors.text, fontWeight: "500" }}>{value ?? "—"}</Text>
    </View>
  );
}

export default function Profile() {
  const { logout } = useAuth();
  const { me, status, isIssuer, activeCommunityId, communities } = useCommunity();
  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useFocusEffect(
    useCallback(() => {
      profileApi.getMine().then(setProfile).catch((e) => setError(e.message));
    }, [])
  );

  if (!profile && !error) return <Loading />;

  const username = profile?.username ?? "";
  const imageUrl = profile?.imageUrl ?? null;
  const firstName = profile?.firstName ?? null;
  const lastName = profile?.lastName ?? null;
  const phone = profile?.phone ?? null;
  const contactEmail = profile?.contactEmail ?? null;

  return (
    <Screen style={{ padding: 0 }}>
      <ScrollView contentContainerStyle={{ padding: spacing.l }}>
        <ErrorText error={error} />
        <Card style={{ alignItems: "center" }}>
          {imageUrl ? (
            <Image
              source={authorizedFile(imageUrl)}
              style={{ width: 88, height: 88, borderRadius: 44, marginBottom: spacing.m }}
            />
          ) : (
            <View
              style={{
                width: 88,
                height: 88,
                borderRadius: 44,
                backgroundColor: colors.border,
                alignItems: "center",
                justifyContent: "center",
                marginBottom: spacing.m,
              }}
            >
              <Text style={{ fontSize: 32, color: colors.muted }}>
                {username.slice(0, 1).toUpperCase()}
              </Text>
            </View>
          )}
          <Text style={{ fontSize: 20, fontWeight: "700", color: colors.text }}>{username}</Text>
          {me && (
            <Text style={{ color: colors.muted, marginTop: spacing.xs }}>
              {communities.find((c) => c.id === activeCommunityId)?.name}
              {status === "unconfirmed" ? " · nepotvrđen" : ""}
            </Text>
          )}
        </Card>

        <Card>
          <SectionTitle>Lični podaci</SectionTitle>
          <Row label="Ime" value={firstName} />
          <Row label="Prezime" value={lastName} />
          <Row label="Telefon" value={phone} />
          <Row label="Kontakt mejl" value={contactEmail} />
          {me && <Row label="Broj posebnog dela" value={me.unitNumber} />}
          {me && me.isConfirmed && <Row label="Zvezdice" value={`⭐ ${me.stars ?? 0}`} />}
          {me && me.roles.length > 0 && <Row label="Uloge" value={me.roles.map(roleLabel).join(", ")} />}
        </Card>

        <Button title="Izmeni profil" onPress={() => router.push("/profile-edit")} />

        {isIssuer && (
          <Button
            title="Izdaj potvrdu (QR)"
            variant="secondary"
            onPress={() => router.push("/certify-show")}
            style={{ marginTop: spacing.m }}
          />
        )}

        <Button
          title="Odjavi se"
          variant="danger"
          onPress={logout}
          style={{ marginTop: spacing.xl }}
        />
      </ScrollView>
    </Screen>
  );
}
