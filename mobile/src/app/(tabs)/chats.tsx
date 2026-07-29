import { router, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { Alert, FlatList, Pressable, ScrollView, Text, View } from "react-native";
import { chatApi } from "../../api/chat";
import { memberApi } from "../../api/community";
import type { ChatSummaryDto, MemberSummaryDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, EmptyState, ErrorText, Screen, SectionTitle } from "../../ui/Basics";
import { formatDateTime, helpStatusLabel } from "../../ui/labels";
import { colors, spacing } from "../../ui/theme";

type Segment = "direct" | "help" | "temporary";

function ChatRow({ chat, subtitle }: { chat: ChatSummaryDto; subtitle?: string }) {
  return (
    <Pressable
      onPress={() => router.push({ pathname: "/chat/[chatId]", params: { chatId: chat.id } })}
    >
      <Card style={{ padding: spacing.m, marginBottom: spacing.s }}>
        <View style={{ flexDirection: "row", alignItems: "center" }}>
          <View style={{ flex: 1 }}>
            <Text style={{ fontWeight: chat.hasUnread ? "800" : "600", color: colors.text }}>
              {chat.participantUsernames.join(", ") || "Čet"}
            </Text>
            <Text style={{ color: colors.muted, fontSize: 12, marginTop: 2 }}>
              {subtitle ?? formatDateTime(chat.lastActivityAt)}
            </Text>
          </View>
          {chat.hasUnread && (
            <View
              style={{ width: 10, height: 10, borderRadius: 5, backgroundColor: colors.primary }}
            />
          )}
        </View>
      </Card>
    </Pressable>
  );
}

function UnconfirmedChats() {
  const { activeCommunityId } = useCommunity();
  const [issuers, setIssuers] = useState<MemberSummaryDto[]>([]);
  const [chats, setChats] = useState<ChatSummaryDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useFocusEffect(
    useCallback(() => {
      if (!activeCommunityId) return;
      Promise.all([
        memberApi.getIssuers(activeCommunityId),
        chatApi.getTemporaryPage(activeCommunityId, null),
      ])
        .then(([loadedIssuers, page]) => {
          setIssuers(loadedIssuers);
          setChats(page.items);
        })
        .catch((e) => setError(e.message));
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
      <Card>
        <Text style={{ color: colors.text, lineHeight: 20 }}>
          Još uvek nisi potvrđen član ove zajednice. Dogovori se sa nekim od izdavača potvrde i
          nađite se uživo — potvrda se izdaje skeniranjem QR koda sa njegovog telefona.
        </Text>
      </Card>
      <Button title="Skeniraj QR kod za potvrdu" onPress={() => router.push("/certify-scan")} />
      {chats.length > 0 && (
        <>
          <SectionTitle>Tvoji razgovori</SectionTitle>
          {chats.map((chat) => (
            <ChatRow key={chat.id} chat={chat} />
          ))}
        </>
      )}
      <SectionTitle>Izdavači potvrde</SectionTitle>
      <ErrorText error={error} />
      {issuers.map((issuer) => (
        <Pressable key={issuer.membershipId} onPress={() => openChat(issuer)}>
          <Card style={{ padding: spacing.m, marginBottom: spacing.s }}>
            <Text style={{ fontWeight: "600", color: colors.text }}>{issuer.username}</Text>
            <Text style={{ color: colors.muted, fontSize: 12, marginTop: 2 }}>
              Započni razgovor o potvrđivanju
            </Text>
          </Card>
        </Pressable>
      ))}
    </ScrollView>
  );
}

export default function Chats() {
  const { activeCommunityId, status, isIssuer } = useCommunity();
  const [segment, setSegment] = useState<Segment>("direct");
  const [chats, setChats] = useState<ChatSummaryDto[]>([]);
  const [cursor, setCursor] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fetchPage = useCallback(
    (before: string | null) => {
      if (!activeCommunityId) return Promise.resolve(null);
      if (segment === "direct") return chatApi.getDirectPage(activeCommunityId, before);
      if (segment === "help") return chatApi.getHelpRequestPage(activeCommunityId, before);
      return chatApi.getTemporaryPage(activeCommunityId, before);
    },
    [activeCommunityId, segment]
  );

  const reload = useCallback(() => {
    setError(null);
    fetchPage(null)
      .then((page) => {
        if (!page) return;
        setChats(page.items);
        setCursor(page.nextCursor);
      })
      .catch((e) => setError(e.message));
  }, [fetchPage]);

  useFocusEffect(
    useCallback(() => {
      if (status === "confirmed") reload();
    }, [reload, status])
  );

  if (status === "unconfirmed") return <UnconfirmedChats />;

  const segments: { value: Segment; label: string }[] = [
    { value: "direct", label: "Članovi" },
    { value: "help", label: "Ispomoći" },
    ...(isIssuer ? [{ value: "temporary" as Segment, label: "Izdavanja" }] : []),
  ];

  return (
    <Screen>
      <View style={{ flexDirection: "row", marginBottom: spacing.m, gap: spacing.s }}>
        {segments.map(({ value, label }) => (
          <Pressable
            key={value}
            onPress={() => setSegment(value)}
            style={{
              flex: 1,
              paddingVertical: spacing.s,
              borderRadius: 8,
              alignItems: "center",
              backgroundColor: segment === value ? colors.primary : colors.card,
              borderWidth: 1,
              borderColor: segment === value ? colors.primary : colors.border,
            }}
          >
            <Text style={{ color: segment === value ? "#fff" : colors.text, fontWeight: "600" }}>
              {label}
            </Text>
          </Pressable>
        ))}
      </View>
      <ErrorText error={error} />
      <FlatList
        data={chats}
        keyExtractor={(c) => c.id}
        renderItem={({ item }) => (
          <ChatRow
            chat={item}
            subtitle={
              segment === "help"
                ? `ispomoć · ${helpStatusLabel(item.status)}`
                : undefined
            }
          />
        )}
        onEndReached={() => {
          if (!cursor) return;
          fetchPage(cursor)
            .then((page) => {
              if (!page) return;
              setChats((current) => [...current, ...page.items]);
              setCursor(page.nextCursor);
            })
            .catch(() => {});
        }}
        onEndReachedThreshold={0.4}
        ListEmptyComponent={<EmptyState text="Nema razgovora u ovoj kategoriji." />}
      />
    </Screen>
  );
}
