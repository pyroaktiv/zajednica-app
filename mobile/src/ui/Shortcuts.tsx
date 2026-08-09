import { Ionicons } from "@expo/vector-icons";
import { router, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { Alert, Pressable, ScrollView, Text, View } from "react-native";
import { chatApi } from "../api/chat";
import { memberApi } from "../api/community";
import type { MemberSummaryDto } from "../api/types";
import { useCommunity } from "../state/CommunityContext";
import { Button, Card, ErrorText, Screen, SectionTitle } from "./Basics";
import { colors, spacing } from "./theme";

export function JoinCommunityShortcut() {
  return (
    <Screen style={{ justifyContent: "center" }}>
      <View style={{ alignItems: "center", marginBottom: spacing.xl }}>
        <Ionicons name="qr-code-outline" size={72} color={colors.primary} />
        <Text
          style={{
            fontSize: 18,
            fontWeight: "700",
            color: colors.text,
            textAlign: "center",
            marginTop: spacing.l,
          }}
        >
          Nisi član nijedne stambene zajednice
        </Text>
        <Text style={{ color: colors.muted, textAlign: "center", marginTop: spacing.s }}>
          Zajednici se pristupa skeniranjem njenog QR koda. Potraži ga u ulazu zgrade.
        </Text>
      </View>
      <Button title="Skeniraj QR kod zajednice" onPress={() => router.push("/join-scan")} />
    </Screen>
  );
}

export function CertificationShortcut() {
  const { activeCommunityId } = useCommunity();
  const [issuers, setIssuers] = useState<MemberSummaryDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useFocusEffect(
    useCallback(() => {
      if (!activeCommunityId) return;
      memberApi.getIssuers(activeCommunityId).then(setIssuers).catch((e) => setError(e.message));
    }, [activeCommunityId])
  );

  const openChat = async (issuer: MemberSummaryDto) => {
    try {
      const chat = await chatApi.openTemporary(activeCommunityId!, issuer.membershipId);
      router.push({ pathname: "/chat/[chatId]", params: { chatId: chat.id } });
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    }
  };

  return (
    <ScrollView contentContainerStyle={{ padding: spacing.l }}>
      <View style={{ alignItems: "center", marginVertical: spacing.l }}>
        <Ionicons name="shield-checkmark-outline" size={64} color={colors.primary} />
        <Text
          style={{
            fontSize: 18,
            fontWeight: "700",
            color: colors.text,
            textAlign: "center",
            marginTop: spacing.l,
          }}
        >
          Još uvek nisi potvrđen član
        </Text>
        <Text style={{ color: colors.muted, textAlign: "center", marginTop: spacing.s }}>
          Dogovori se sa nekim od izdavača potvrde i nađite se uživo. Potvrda se izdaje
          skeniranjem QR koda sa njegovog telefona.
        </Text>
      </View>
      <Button style={{marginBottom: spacing.l}} title="Skeniraj QR kod za potvrdu" onPress={() => router.push("/certify-scan")} />
      <SectionTitle>Izaberi izdavača potvrde</SectionTitle>
      <ErrorText error={error} />
      {issuers.map((issuer) => (
        <Pressable key={issuer.membershipId} onPress={() => openChat(issuer)}>
          <Card style={{ padding: spacing.m, marginBottom: spacing.s }}>
            <Text style={{ fontWeight: "600", color: colors.text }}>{issuer.username}</Text>
          </Card>
        </Pressable>
      ))}
    </ScrollView>
  );
}
