import { Image } from "expo-image";
import { router, Stack, useLocalSearchParams } from "expo-router";
import { useEffect, useState } from "react";
import { Alert, ScrollView, Text, View } from "react-native";
import { chatApi } from "../../api/chat";
import { authorizedFile } from "../../api/client";
import { memberApi } from "../../api/community";
import type { MemberProfileDto } from "../../api/types";
import { RoleNames } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, ErrorText, Loading, Screen, SectionTitle } from "../../ui/Basics";
import { formatDateTime } from "../../ui/labels";
import { roleLabel } from "../../ui/MemberRow";
import { MutedNotice } from "../../ui/MutedNotice";
import { colors, spacing } from "../../ui/theme";

function Row({ label, value }: { label: string; value: string | null }) {
  return (
    <View style={{ flexDirection: "row", justifyContent: "space-between", marginBottom: spacing.s }}>
      <Text style={{ color: colors.muted }}>{label}</Text>
      <Text style={{ color: colors.text, fontWeight: "500" }}>{value ?? "—"}</Text>
    </View>
  );
}

export default function MemberProfile() {
  const { membershipId } = useLocalSearchParams<{ membershipId: string }>();
  const { activeCommunityId, me, isIssuer, isMuted } = useCommunity();
  const [member, setMember] = useState<MemberProfileDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const load = () => {
    if (!activeCommunityId || !membershipId) return;
    memberApi.get(activeCommunityId, membershipId).then(setMember).catch((e) => setError(e.message));
  };

  useEffect(load, [activeCommunityId, membershipId]);

  if (error && !member) {
    return (
      <Screen>
        <Stack.Screen options={{ title: "Profil člana" }} />
        <ErrorText error={error} />
      </Screen>
    );
  }
  if (!member) return <Loading />;

  const isMe = me?.membershipId === member.membershipId;
  const openChat = async () => {
    setBusy(true);
    try {
      const chat = await chatApi.openDirect(activeCommunityId!, member.membershipId);
      router.push({ pathname: "/chat/[chatId]", params: { chatId: chat.id } });
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const grantIssuer = () => {
    Alert.alert(
      "Dodela prava",
      `Dodeliti korisniku ${member.username} pravo izdavanja potvrda?`,
      [
        { text: "Odustani", style: "cancel" },
        {
          text: "Dodeli",
          onPress: async () => {
            try {
              await memberApi.grantIssuer(activeCommunityId!, member.membershipId);
              load();
            } catch (e: any) {
              Alert.alert("Greška", e.message);
            }
          },
        },
      ]
    );
  };

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: member.username }} />
      <ScrollView contentContainerStyle={{ padding: spacing.l }}>
        <Card style={{ alignItems: "center" }}>
          {member.imageUrl ? (
            <Image
              source={authorizedFile(member.imageUrl)}
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
                {member.username.slice(0, 1).toUpperCase()}
              </Text>
            </View>
          )}
          <Text style={{ fontSize: 20, fontWeight: "700", color: colors.text }}>
            {member.username}
          </Text>
          <Text style={{ color: colors.muted, marginTop: spacing.xs }}>
            {member.roles.length > 0
              ? member.roles.map(roleLabel).join(" · ")
              : member.isConfirmed
                ? "potvrđen član"
                : "nepotvrđen član"}
            {member.state !== "Active" ? " · istupio iz zajednice" : ""}
          </Text>
          {member.mutedUntil && (
            <Text style={{ color: colors.warning, marginTop: spacing.xs, fontWeight: "600" }}>
              utišan do {formatDateTime(member.mutedUntil)}
            </Text>
          )}
        </Card>

        <Card>
          <SectionTitle>Podaci</SectionTitle>
          <Row label="Ime" value={member.firstName} />
          <Row label="Prezime" value={member.lastName} />
          <Row label="Telefon" value={member.phone} />
          <Row label="Kontakt mejl" value={member.contactEmail} />
          <Row label="Broj posebnog dela" value={member.unitNumber} />
          {member.isConfirmed && <Row label="Zvezdice" value={`⭐ ${member.stars ?? 0}`} />}
          <Row label="Član od" value={new Date(member.dateJoined).toLocaleDateString("sr-RS")} />
        </Card>

        {!isMe && member.state === "Active" && (
          <>
            <MutedNotice />
            {!isMuted && <Button title="Pošalji poruku" onPress={openChat} loading={busy} />}
            {member.isConfirmed && !isMuted && (
              <Button
                title="Otvori nameru"
                variant="secondary"
                onPress={() =>
                  router.push({
                    pathname: "/intent-create",
                    params: { targetMembershipId: member.membershipId, username: member.username },
                  })
                }
                style={{ marginTop: spacing.m }}
              />
            )}
            {isIssuer && member.isConfirmed && !member.roles.includes(RoleNames.Issuer) && (
              <Button
                title="Dodeli mogućnost izdavanja potvrda"
                variant="secondary"
                onPress={grantIssuer}
                style={{ marginTop: spacing.m }}
              />
            )}
          </>
        )}
      </ScrollView>
    </Screen>
  );
}
