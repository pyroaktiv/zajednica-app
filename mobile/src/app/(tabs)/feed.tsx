import { router, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { FlatList, Pressable, Text, View } from "react-native";
import { intentApi, postApi } from "../../api/feed";
import type { IntentSummaryDto, PostDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, EmptyState, ErrorText, Screen } from "../../ui/Basics";
import { formatDateTime, intentKindLabel, intentStatusLabel } from "../../ui/labels";
import { PostCard } from "../../ui/PostCard";
import { MutedNotice } from "../../ui/MutedNotice";
import { CertificationShortcut, JoinCommunityShortcut } from "../../ui/Shortcuts";
import { colors, spacing } from "../../ui/theme";

type Segment = "general" | "intents";

function IntentCard({ intent }: { intent: IntentSummaryDto }) {
  return (
    <Pressable
      onPress={() => router.push({ pathname: "/intent/[intentId]", params: { intentId: intent.id } })}
    >
      <Card style={{ marginBottom: spacing.s, padding: spacing.m }}>
        <View style={{ flexDirection: "row", alignItems: "center", marginBottom: spacing.xs }}>
          <Text style={{ fontWeight: "700", color: colors.text, flex: 1 }}>
            {intentKindLabel(intent.kind)}: {intent.targetUsername ?? "?"}
          </Text>
          <Text
            style={{
              color: intent.status === "Open" ? colors.primary : colors.muted,
              fontSize: 12,
              fontWeight: "600",
            }}
          >
            {intentStatusLabel(intent.status)}
          </Text>
        </View>
        <Text style={{ color: colors.text }} numberOfLines={3}>
          {intent.text}
        </Text>
        <Text style={{ color: colors.muted, fontSize: 12, marginTop: spacing.s }}>
          {formatDateTime(intent.dateCreated)} · rok {formatDateTime(intent.deadline)}
        </Text>
      </Card>
    </Pressable>
  );
}

export default function Feed() {
  const { activeCommunityId, status, isMuted } = useCommunity();
  const [segment, setSegment] = useState<Segment>("general");
  const [posts, setPosts] = useState<PostDto[]>([]);
  const [postsCursor, setPostsCursor] = useState<string | null>(null);
  const [intents, setIntents] = useState<IntentSummaryDto[]>([]);
  const [intentsCursor, setIntentsCursor] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const reload = useCallback(async () => {
    if (!activeCommunityId || status !== "confirmed") return;
    setError(null);
    try {
      if (segment === "general") {
        const page = await postApi.getPage(activeCommunityId, null);
        setPosts(page.items);
        setPostsCursor(page.nextCursor);
      } else {
        const page = await intentApi.getPage(activeCommunityId, null);
        setIntents(page.items);
        setIntentsCursor(page.nextCursor);
      }
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId, segment, status]);

  const loadMore = async () => {
    if (!activeCommunityId) return;
    try {
      if (segment === "general" && postsCursor) {
        const page = await postApi.getPage(activeCommunityId, postsCursor);
        setPosts((current) => [...current, ...page.items]);
        setPostsCursor(page.nextCursor);
      } else if (segment === "intents" && intentsCursor) {
        const page = await intentApi.getPage(activeCommunityId, intentsCursor);
        setIntents((current) => [...current, ...page.items]);
        setIntentsCursor(page.nextCursor);
      }
    } catch {}
  };

  useFocusEffect(
    useCallback(() => {
      reload();
    }, [reload])
  );

  if (status === "none") return <JoinCommunityShortcut />;
  if (status === "unconfirmed") return <CertificationShortcut />;

  return (
    <Screen>
      <View style={{ flexDirection: "row", marginBottom: spacing.m, gap: spacing.s }}>
        {(["general", "intents"] as Segment[]).map((value) => (
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
              {value === "general" ? "Generalno" : "Namere"}
            </Text>
          </Pressable>
        ))}
      </View>
      <ErrorText error={error} />
      {segment === "general" ? (
        <>
          <FlatList
            data={posts}
            keyExtractor={(p) => p.id}
            renderItem={({ item }) => <PostCard post={item} />}
            onEndReached={loadMore}
            onEndReachedThreshold={0.4}
            ListEmptyComponent={<EmptyState text="Još nema objava." />}
          />
          <MutedNotice />
          {!isMuted && (
            <Button
              title="Kreiraj novu objavu"
              onPress={() => router.push("/post-create")}
              style={{ marginTop: spacing.s }}
            />
          )}
        </>
      ) : (
        <FlatList
          data={intents}
          keyExtractor={(i) => i.id}
          renderItem={({ item }) => <IntentCard intent={item} />}
          onEndReached={loadMore}
          onEndReachedThreshold={0.4}
          ListEmptyComponent={<EmptyState text="Još nema namera." />}
        />
      )}
    </Screen>
  );
}
