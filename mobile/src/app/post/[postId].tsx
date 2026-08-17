import { Image } from "expo-image";
import { router, Stack, useLocalSearchParams } from "expo-router";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  Alert,
  KeyboardAvoidingView,
  Modal,
  type NativeScrollEvent,
  type NativeSyntheticEvent,
  Platform,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { helpChatApi } from "../../api/chat";
import { authorizedFile } from "../../api/client";
import { commentApi, intentApi, postApi } from "../../api/feed";
import type { CommentDto, PostDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, EmptyState, ErrorText, Loading, Screen, SectionTitle } from "../../ui/Basics";
import { MutedNotice } from "../../ui/MutedNotice";
import { formatDateTime, postKindLabel, ratingZoneColor, ratingZoneLabel } from "../../ui/labels";
import { colors, spacing } from "../../ui/theme";

const MAX_INDENT_DEPTH = 4;

function CommentView({
  comment,
  depth,
  onReply,
  replies,
  repliesByComment,
  onLoadReplies,
  repliesCursor,
  onLoadMoreReplies,
}: {
  comment: CommentDto;
  depth: number;
  onReply: (comment: CommentDto) => void;
  replies: CommentDto[] | undefined;
  repliesByComment: Record<string, CommentDto[]>;
  onLoadReplies: (comment: CommentDto) => void;
  repliesCursor: Record<string, string | null>;
  onLoadMoreReplies: (comment: CommentDto) => void;
}) {
  const hasMoreReplies = repliesCursor[comment.id] != null;
  return (
    <View style={{ marginBottom: spacing.s }}>
      <View
        style={{
          backgroundColor: colors.background,
          borderRadius: 8,
          padding: spacing.m,
          borderWidth: 1,
          borderColor: colors.border,
        }}
      >
        <Text style={{ fontWeight: "600", color: colors.text, fontSize: 13 }}>
          {comment.authorUsername}
          <Text style={{ color: colors.muted, fontWeight: "400" }}>
            {"  "}
            {formatDateTime(comment.date)}
          </Text>
        </Text>
        <Text style={{ color: colors.text, marginTop: 4 }}>{comment.text}</Text>
        <View style={{ flexDirection: "row", gap: spacing.l, marginTop: spacing.s }}>
          <Pressable onPress={() => onReply(comment)}>
            <Text style={{ color: colors.primary, fontSize: 12 }}>Odgovori</Text>
          </Pressable>
          {comment.hasReplies && !replies && (
            <Pressable onPress={() => onLoadReplies(comment)}>
              <Text style={{ color: colors.primary, fontSize: 12 }}>Prikaži odgovore</Text>
            </Pressable>
          )}
        </View>
      </View>
      {replies && (
        <View style={{ marginLeft: depth < MAX_INDENT_DEPTH ? spacing.xl : 0, marginTop: spacing.s }}>
          {replies.map((reply) => (
            <CommentView
              key={reply.id}
              comment={reply}
              depth={depth + 1}
              onReply={onReply}
              replies={repliesByComment[reply.id]}
              repliesByComment={repliesByComment}
              onLoadReplies={onLoadReplies}
              repliesCursor={repliesCursor}
              onLoadMoreReplies={onLoadMoreReplies}
            />
          ))}
          {hasMoreReplies && (
            <Pressable onPress={() => onLoadMoreReplies(comment)}>
              <Text style={{ color: colors.primary, fontSize: 12, marginTop: spacing.xs }}>
                Prikaži još odgovora
              </Text>
            </Pressable>
          )}
        </View>
      )}
    </View>
  );
}

export default function PostDetails() {
  const { postId } = useLocalSearchParams<{ postId: string }>();
  const { activeCommunityId, me, isMuted } = useCommunity();
  const [post, setPost] = useState<PostDto | null>(null);
  const [comments, setComments] = useState<CommentDto[]>([]);
  const [commentsCursor, setCommentsCursor] = useState<string | null>(null);
  const [replies, setReplies] = useState<Record<string, CommentDto[]>>({});
  const [repliesCursor, setRepliesCursor] = useState<Record<string, string | null>>({});
  const [commentText, setCommentText] = useState("");
  const [replyTo, setReplyTo] = useState<CommentDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [zoomedImageUrl, setZoomedImageUrl] = useState<string | null>(null);
  const loadingComments = useRef(false);

  const isGeneral = post?.type === "GENERAL";
  const isHelp = post?.type === "HELP_REQUEST";
  const isAuthor = post != null && me?.membershipId === post.authorMembershipId;

  const load = useCallback(async () => {
    if (!activeCommunityId || !postId) return;
    try {
      const loaded = await postApi.get(activeCommunityId, postId);
      setPost(loaded);
      if (loaded.type === "GENERAL") {
        const page = await commentApi.getRoots(activeCommunityId, postId, null);
        setComments(page.items);
        setCommentsCursor(page.nextCursor);
      }
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId, postId]);

  const loadMoreComments = useCallback(async () => {
    if (!activeCommunityId || !postId || !commentsCursor || loadingComments.current) return;
    loadingComments.current = true;
    try {
      const page = await commentApi.getRoots(activeCommunityId, postId, commentsCursor);
      setComments((current) => [...current, ...page.items]);
      setCommentsCursor(page.nextCursor);
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      loadingComments.current = false;
    }
  }, [activeCommunityId, postId, commentsCursor]);

  const onScroll = (event: NativeSyntheticEvent<NativeScrollEvent>) => {
    const { layoutMeasurement, contentOffset, contentSize } = event.nativeEvent;
    if (contentOffset.y + layoutMeasurement.height >= contentSize.height - 240) {
      loadMoreComments();
    }
  };

  useEffect(() => {
    load();
  }, [load]);

  if (error && !post) {
    return (
      <Screen>
        <Stack.Screen options={{ title: "Objava" }} />
        <ErrorText error={error} />
      </Screen>
    );
  }
  if (!post) return <Loading />;

  const kind = postKindLabel(post.kind);

  const submitComment = async () => {
    if (!activeCommunityId || !commentText.trim()) return;
    setBusy(true);
    try {
      if (replyTo) {
        const created = await commentApi.reply(activeCommunityId, post.id, replyTo.id, commentText.trim());
        setReplies((current) => ({
          ...current,
          [replyTo.id]: [...(current[replyTo.id] ?? []), created],
        }));
        setReplyTo(null);
      } else {
        const created = await commentApi.add(activeCommunityId, post.id, commentText.trim());
        setComments((current) => [...current, created]);
      }
      setCommentText("");
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const loadReplies = async (comment: CommentDto) => {
    if (!activeCommunityId) return;
    try {
      const page = await commentApi.getReplies(activeCommunityId, post.id, comment.id, null);
      setReplies((current) => ({ ...current, [comment.id]: page.items }));
      setRepliesCursor((current) => ({ ...current, [comment.id]: page.nextCursor }));
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    }
  };

  const loadMoreReplies = async (comment: CommentDto) => {
    if (!activeCommunityId) return;
    const cursor = repliesCursor[comment.id];
    if (!cursor) return;
    try {
      const page = await commentApi.getReplies(activeCommunityId, post.id, comment.id, cursor);
      setReplies((current) => ({
        ...current,
        [comment.id]: [...(current[comment.id] ?? []), ...page.items],
      }));
      setRepliesCursor((current) => ({ ...current, [comment.id]: page.nextCursor }));
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    }
  };

  const respond = async () => {
    if (!activeCommunityId) return;
    setBusy(true);
    try {
      const chat = await helpChatApi.respond(activeCommunityId, post.id);
      router.push({ pathname: "/chat/[chatId]", params: { chatId: chat.id } });
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const close = async () => {
    if (!activeCommunityId) return;
    setBusy(true);
    try {
      setPost(await postApi.closeHelpRequest(activeCommunityId, post.id));
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const startRating = async () => {
    if (!activeCommunityId) return;
    setBusy(true);
    try {
      const opened = await intentApi.openPostRating(activeCommunityId, post.id);
      router.push({ pathname: "/intent/[intentId]", params: { intentId: opened.id } });
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const confirmRating = () => {
    Alert.alert(
      "Ocena zajednice",
      "Pokrenuti glasanje o stavu iznesenom u ovoj objavi?",
      [
        { text: "Odustani", style: "cancel" },
        { text: "Pokreni", onPress: startRating },
      ]
    );
  };

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: isHelp ? "Ispomoć" : "Objava" }} />
      <KeyboardAvoidingView
        style={{ flex: 1 }}
        behavior={Platform.OS === "ios" ? "padding" : undefined}
      >
        <ScrollView contentContainerStyle={{ padding: spacing.l }} onScroll={onScroll} scrollEventThrottle={100}>
          <Card>
            <View style={{ flexDirection: "row", alignItems: "center", marginBottom: spacing.s }}>
              <Text style={{ fontWeight: "700", color: colors.text, flex: 1 }}>
                {post.authorUsername}
              </Text>
              {kind && <Text style={{ color: kind.color, fontWeight: "700" }}>{kind.text}</Text>}
              {isHelp && (
                <Text style={{ color: post.closed ? colors.muted : colors.primary, fontWeight: "700" }}>
                  {post.closed ? "Zatvorena" : "Otvorena ispomoć"}
                </Text>
              )}
            </View>
            <Text style={{ color: colors.muted, fontSize: 12, marginBottom: spacing.s }}>
              {formatDateTime(post.dateCreated)}
            </Text>
            <Text style={{ color: colors.text, fontSize: 15, lineHeight: 22 }}>{post.text}</Text>
            {post.imageUrls.length > 0 && (
              <View style={{ flexDirection: "row", flexWrap: "wrap", gap: spacing.s, marginTop: spacing.m }}>
                {post.imageUrls.map((url) => (
                  <Pressable key={url} onPress={() => setZoomedImageUrl(url)}>
                    <Image
                      source={authorizedFile(url)}
                      style={{ width: 96, height: 96, borderRadius: 8 }}
                      contentFit="cover"
                    />
                  </Pressable>
                ))}
              </View>
            )}
            {isGeneral && post.rating && (
              <View style={{ flexDirection: "row", marginTop: spacing.m }}>
                <Text
                  style={{
                    backgroundColor: ratingZoneColor(post.rating.zone).background,
                    color: ratingZoneColor(post.rating.zone).text,
                    fontWeight: "700",
                    fontSize: 12,
                    borderRadius: 6,
                    paddingHorizontal: 8,
                    paddingVertical: 4,
                    overflow: "hidden",
                  }}
                >
                  Ocena zajednice · {ratingZoneLabel(post.rating.zone)} ({post.rating.approvalPercentage}%)
                </Text>
              </View>
            )}
          </Card>

          <MutedNotice />
          {isHelp && !post.closed && !isAuthor && !isMuted && (
            <Button title="Priskoči u pomoć" onPress={respond} loading={busy} />
          )}
          {isHelp && !post.closed && isAuthor && (
            <Button title="Zatvori ispomoć" variant="danger" onPress={close} loading={busy} />
          )}
          {isGeneral && !isMuted && !post.rating && (
            <Button
              title="Pokreni ocenu zajednice"
              variant="secondary"
              onPress={confirmRating}
              loading={busy}
            />
          )}

          {isGeneral && (
            <>
              <SectionTitle>Komentari</SectionTitle>
              {comments.length === 0 && <EmptyState text="Još nema komentara." />}
              {comments.map((comment) => (
                <CommentView
                  key={comment.id}
                  comment={comment}
                  depth={0}
                  onReply={setReplyTo}
                  replies={replies[comment.id]}
                  repliesByComment={replies}
                  onLoadReplies={loadReplies}
                  repliesCursor={repliesCursor}
                  onLoadMoreReplies={loadMoreReplies}
                />
              ))}
            </>
          )}
        </ScrollView>

        {isGeneral && !isMuted && (
          <View
            style={{
              flexDirection: "row",
              padding: spacing.m,
              backgroundColor: colors.card,
              borderTopWidth: 1,
              borderTopColor: colors.border,
              alignItems: "center",
              gap: spacing.s,
            }}
          >
            <View style={{ flex: 1 }}>
              {replyTo && (
                <Pressable onPress={() => setReplyTo(null)}>
                  <Text style={{ color: colors.muted, fontSize: 11 }}>
                    Odgovor za {replyTo.authorUsername} ✕
                  </Text>
                </Pressable>
              )}
              <TextInput
                value={commentText}
                onChangeText={setCommentText}
                placeholder={replyTo ? "Napiši odgovor..." : "Napiši komentar..."}
                placeholderTextColor={colors.muted}
                style={{ color: colors.text, paddingVertical: 6 }}
              />
            </View>
            <Button title="Pošalji" onPress={submitComment} loading={busy} />
          </View>
        )}
      </KeyboardAvoidingView>

      <Modal
        visible={zoomedImageUrl != null}
        transparent
        animationType="fade"
        onRequestClose={() => setZoomedImageUrl(null)}
      >
        <Pressable
          style={{
            flex: 1,
            backgroundColor: "rgba(0,0,0,0.92)",
            alignItems: "center",
            justifyContent: "center",
          }}
          onPress={() => setZoomedImageUrl(null)}
        >
          {zoomedImageUrl && (
            <Image
              source={authorizedFile(zoomedImageUrl)}
              style={{ width: "100%", height: "100%" }}
              contentFit="contain"
            />
          )}
        </Pressable>
      </Modal>
    </Screen>
  );
}
