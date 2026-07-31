import { router, Stack, useLocalSearchParams } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import {
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { helpChatApi } from "../../api/chat";
import { commentApi, postApi } from "../../api/feed";
import type { CommentDto, PostDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { Button, Card, EmptyState, ErrorText, Loading, Screen, SectionTitle } from "../../ui/Basics";
import { MutedNotice } from "../../ui/MutedNotice";
import { formatDateTime, postKindLabel } from "../../ui/labels";
import { colors, spacing } from "../../ui/theme";

function CommentView({
  comment,
  onReply,
  replies,
  onLoadReplies,
}: {
  comment: CommentDto;
  onReply: (comment: CommentDto) => void;
  replies: CommentDto[] | undefined;
  onLoadReplies: (comment: CommentDto) => void;
}) {
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
          {!comment.parentCommentId && (
            <Pressable onPress={() => onReply(comment)}>
              <Text style={{ color: colors.primary, fontSize: 12 }}>Odgovori</Text>
            </Pressable>
          )}
          {comment.hasReplies && !replies && (
            <Pressable onPress={() => onLoadReplies(comment)}>
              <Text style={{ color: colors.primary, fontSize: 12 }}>Prikaži odgovore</Text>
            </Pressable>
          )}
        </View>
      </View>
      {replies && (
        <View style={{ marginLeft: spacing.xl, marginTop: spacing.s }}>
          {replies.map((reply) => (
            <CommentView
              key={reply.id}
              comment={reply}
              onReply={onReply}
              replies={undefined}
              onLoadReplies={onLoadReplies}
            />
          ))}
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
  const [replies, setReplies] = useState<Record<string, CommentDto[]>>({});
  const [commentText, setCommentText] = useState("");
  const [replyTo, setReplyTo] = useState<CommentDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

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
      }
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId, postId]);

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

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: isHelp ? "Ispomoć" : "Objava" }} />
      <KeyboardAvoidingView
        style={{ flex: 1 }}
        behavior={Platform.OS === "ios" ? "padding" : undefined}
      >
        <ScrollView contentContainerStyle={{ padding: spacing.l }}>
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
            {post.imageUrls.map((url) => (
              <Image
                key={url}
                source={{ uri: url }}
                style={{ width: "100%", height: 220, borderRadius: 8, marginTop: spacing.m }}
                resizeMode="cover"
              />
            ))}
          </Card>

          <MutedNotice />
          {isHelp && !post.closed && !isAuthor && !isMuted && (
            <Button title="Priskoči u pomoć" onPress={respond} loading={busy} />
          )}
          {isHelp && !post.closed && isAuthor && (
            <Button title="Zatvori ispomoć" variant="danger" onPress={close} loading={busy} />
          )}

          {isGeneral && (
            <>
              <SectionTitle>Komentari</SectionTitle>
              {comments.length === 0 && <EmptyState text="Još nema komentara." />}
              {comments.map((comment) => (
                <CommentView
                  key={comment.id}
                  comment={comment}
                  onReply={setReplyTo}
                  replies={replies[comment.id]}
                  onLoadReplies={loadReplies}
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
    </Screen>
  );
}
