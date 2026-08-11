import { Ionicons } from "@expo/vector-icons";
import { router, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { FlatList, Pressable, Text, View } from "react-native";
import { chatApi } from "../../api/chat";
import type { ChatSummaryDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Card, EmptyState, ErrorText, Screen } from "../../ui/Basics";
import { formatDateTime, helpStatusLabel } from "../../ui/labels";
import { JoinCommunityShortcut } from "../../ui/Shortcuts";
import { colors, spacing } from "../../ui/theme";

type Segment = "direct" | "help" | "temporary";

function helpRoleIcon(role: string | null) {
  if (role === "Helper")
    return { name: "heart-outline" as const, color: colors.success, label: "Pružaš pomoć" };
  if (role === "Requester")
    return { name: "help-buoy-outline" as const, color: colors.primary, label: "Primaš pomoć" };
  return null;
}

function ChatRow({
  chat,
  subtitle,
  roleIcon,
}: {
  chat: ChatSummaryDto;
  subtitle?: string;
  roleIcon?: ReturnType<typeof helpRoleIcon>;
}) {
  return (
    <Pressable
      onPress={() => router.push({ pathname: "/chat/[chatId]", params: { chatId: chat.id } })}
    >
      <Card style={{ padding: spacing.m, marginBottom: spacing.s }}>
        <View style={{ flexDirection: "row", alignItems: "center" }}>
          {roleIcon && (
            <View
              style={{
                width: 36,
                height: 36,
                borderRadius: 18,
                marginRight: spacing.m,
                alignItems: "center",
                justifyContent: "center",
                backgroundColor: colors.background,
              }}
            >
              <Ionicons name={roleIcon.name} size={20} color={roleIcon.color} />
            </View>
          )}
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

export default function Chats() {
  const { activeCommunityId, status, isIssuer } = useCommunity();
  const [segment, setSegment] = useState<Segment>("direct");
  const [chats, setChats] = useState<ChatSummaryDto[]>([]);
  const [cursor, setCursor] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const effectiveSegment: Segment = status === "unconfirmed" ? "temporary" : segment;

  const fetchPage = useCallback(
    (before: string | null) => {
      if (!activeCommunityId) return Promise.resolve(null);
      if (effectiveSegment === "direct") return chatApi.getDirectPage(activeCommunityId, before);
      if (effectiveSegment === "help") return chatApi.getHelpRequestPage(activeCommunityId, before);
      return chatApi.getTemporaryPage(activeCommunityId, before);
    },
    [activeCommunityId, effectiveSegment]
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
      if (status !== "none") reload();
    }, [reload, status])
  );

  if (status === "none") return <JoinCommunityShortcut />;

  const segments: { value: Segment; label: string }[] = [
    { value: "direct", label: "Članovi" },
    { value: "help", label: "Ispomoći" },
    ...(isIssuer ? [{ value: "temporary" as Segment, label: "Izdavanja" }] : []),
  ];

  return (
    <Screen>
      {status === "confirmed" && (
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
      )}
      <ErrorText error={error} />
      <FlatList
        data={chats}
        keyExtractor={(c) => c.id}
        renderItem={({ item }) => (
          <ChatRow
            chat={item}
            roleIcon={effectiveSegment === "help" ? helpRoleIcon(item.viewerRole) : undefined}
            subtitle={
              effectiveSegment === "help"
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
        ListEmptyComponent={
          <EmptyState
            text={
              status === "unconfirmed"
                ? "Još nemaš razgovora sa izdavačima potvrde."
                : "Nema razgovora u ovoj kategoriji."
            }
          />
        }
      />
    </Screen>
  );
}
