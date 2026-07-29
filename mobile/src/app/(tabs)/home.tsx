import { Ionicons } from "@expo/vector-icons";
import * as DocumentPicker from "expo-document-picker";
import { Redirect, router, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { Alert, Linking, Pressable, RefreshControl, ScrollView, Text, View } from "react-native";
import { communityApi, documentApi, memberApi } from "../../api/community";
import { fileApi } from "../../api/files";
import type { CommunityDetailsDto, DocumentDto, MemberSummaryDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, ErrorText, Loading, Screen, SectionTitle } from "../../ui/Basics";
import { colors, spacing } from "../../ui/theme";

function Row({ label, value }: { label: string; value: string | null }) {
  return (
    <View style={{ flexDirection: "row", justifyContent: "space-between", marginBottom: spacing.s }}>
      <Text style={{ color: colors.muted }}>{label}</Text>
      <Text style={{ color: colors.text, fontWeight: "500" }}>{value ?? "—"}</Text>
    </View>
  );
}

export default function Home() {
  const { activeCommunityId, status, isManager } = useCommunity();
  const [community, setCommunity] = useState<CommunityDetailsDto | null>(null);
  const [manager, setManager] = useState<MemberSummaryDto | null>(null);
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [ranking, setRanking] = useState<MemberSummaryDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    if (!activeCommunityId) return;
    setError(null);
    try {
      const [details, mgr, docs, rank] = await Promise.all([
        communityApi.get(activeCommunityId),
        memberApi.getManager(activeCommunityId),
        documentApi.getPaged(activeCommunityId, 1, 50),
        memberApi.getRanking(activeCommunityId),
      ]);
      setCommunity(details);
      setManager(mgr);
      setDocuments(docs.results);
      setRanking(rank);
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId]);

  useFocusEffect(
    useCallback(() => {
      load();
    }, [load])
  );

  if (status !== "confirmed") return <Redirect href="/profile" />;
  if (!community && !error) return <Loading />;

  const addDocument = async () => {
    const picked = await DocumentPicker.getDocumentAsync({ type: "application/pdf" });
    if (picked.canceled) return;
    const asset = picked.assets[0];
    try {
      const uploaded = await fileApi.uploadDocument({
        uri: asset.uri,
        name: asset.name,
        type: asset.mimeType ?? "application/pdf",
      });
      await documentApi.add(activeCommunityId!, asset.name, uploaded.url);
      await load();
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    }
  };

  const removeDocument = (doc: DocumentDto) => {
    Alert.alert("Brisanje dokumenta", `Obrisati "${doc.name}"?`, [
      { text: "Odustani", style: "cancel" },
      {
        text: "Obriši",
        style: "destructive",
        onPress: async () => {
          try {
            await documentApi.remove(activeCommunityId!, doc.id);
            await load();
          } catch (e: any) {
            Alert.alert("Greška", e.message);
          }
        },
      },
    ]);
  };

  return (
    <Screen style={{ padding: 0 }}>
      <ScrollView
        contentContainerStyle={{ padding: spacing.l }}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={async () => {
              setRefreshing(true);
              await load();
              setRefreshing(false);
            }}
          />
        }
      >
        <ErrorText error={error} />
        {community && (
          <Card>
            <View style={{ flexDirection: "row", alignItems: "center", marginBottom: spacing.s }}>
              <Text style={{ fontSize: 20, fontWeight: "800", color: colors.text, flex: 1 }}>
                {community.name}
              </Text>
              <Pressable onPress={() => router.push("/community-qr")} hitSlop={12}>
                <Ionicons name="qr-code-outline" size={24} color={colors.primary} />
              </Pressable>
            </View>
            <Row label="Adresa" value={`${community.address.street} ${community.address.number}`} />
            <Row label="Matični broj" value={community.registrationNumber} />
            <Row label="PIB" value={community.taxId} />
            <Row label="Tekući račun" value={community.bankAccountNumber} />
            <Row label="Upravnik" value={manager ? manager.username : "nije izabran"} />
            {isManager && (
              <Button
                title="Izmeni podatke o zajednici"
                variant="secondary"
                onPress={() => router.push("/community-edit")}
                style={{ marginTop: spacing.s }}
              />
            )}
          </Card>
        )}

        <Card>
          <SectionTitle>Dokumenti</SectionTitle>
          {documents.length === 0 && (
            <Text style={{ color: colors.muted }}>Nema postavljenih dokumenata.</Text>
          )}
          {documents.map((doc) => (
            <View
              key={doc.id}
              style={{ flexDirection: "row", alignItems: "center", paddingVertical: spacing.s }}
            >
              <Ionicons name="document-text-outline" size={20} color={colors.muted} />
              <Pressable style={{ flex: 1, marginLeft: spacing.s }} onPress={() => Linking.openURL(doc.url)}>
                <Text style={{ color: colors.primary }}>{doc.name}</Text>
              </Pressable>
              {isManager && (
                <Pressable onPress={() => removeDocument(doc)} hitSlop={12}>
                  <Ionicons name="trash-outline" size={20} color={colors.danger} />
                </Pressable>
              )}
            </View>
          ))}
          {isManager && (
            <Button
              title="Dodaj dokument (PDF)"
              variant="secondary"
              onPress={addDocument}
              style={{ marginTop: spacing.s }}
            />
          )}
        </Card>

        <Card>
          <SectionTitle>Rang lista stanara</SectionTitle>
          {ranking.map((member, index) => (
            <View
              key={member.membershipId}
              style={{ flexDirection: "row", alignItems: "center", paddingVertical: spacing.s }}
            >
              <Text style={{ width: 28, color: colors.muted }}>{index + 1}.</Text>
              <Text style={{ flex: 1, color: colors.text, fontWeight: "500" }}>{member.username}</Text>
              <Text style={{ color: colors.warning }}>⭐ {member.stars ?? 0}</Text>
            </View>
          ))}
        </Card>
      </ScrollView>
    </Screen>
  );
}
