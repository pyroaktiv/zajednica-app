import { Ionicons } from "@expo/vector-icons";
import {
  AudioModule,
  RecordingPresets,
  useAudioPlayer,
  useAudioRecorder,
} from "expo-audio";
import { router, Stack, useLocalSearchParams } from "expo-router";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  Text,
  TextInput,
  View,
} from "react-native";
import { chatApi, helpChatApi, messageApi } from "../../api/chat";
import { fileApi } from "../../api/files";
import type { ChatDetailsDto, MessageDto } from "../../api/types";
import { useChannel } from "../../realtime/connection";
import { useCommunity } from "../../state/CommunityContext";
import { Button, ErrorText, Loading, Screen } from "../../ui/Basics";
import { helpStatusLabel } from "../../ui/labels";
import { colors, spacing } from "../../ui/theme";

function VoiceBubble({ url, durationSeconds }: { url: string; durationSeconds: number | null }) {
  const player = useAudioPlayer(url);
  return (
    <Pressable
      onPress={() => {
        player.seekTo(0);
        player.play();
      }}
      style={{ flexDirection: "row", alignItems: "center", gap: spacing.s }}
    >
      <Ionicons name="play-circle" size={28} color={colors.primary} />
      <Text style={{ color: colors.text }}>Glasovna poruka ({durationSeconds ?? "?"}s)</Text>
    </Pressable>
  );
}

function MessageBubble({ message, mine }: { message: MessageDto; mine: boolean }) {
  return (
    <View
      style={{
        alignSelf: mine ? "flex-end" : "flex-start",
        maxWidth: "80%",
        backgroundColor: mine ? "#dcebff" : colors.card,
        borderRadius: 12,
        borderWidth: 1,
        borderColor: colors.border,
        padding: spacing.m,
        marginBottom: spacing.s,
      }}
    >
      {!mine && (
        <Text style={{ fontWeight: "700", fontSize: 12, color: colors.primaryDark, marginBottom: 2 }}>
          {message.senderUsername}
        </Text>
      )}
      {message.type === "VOICE" && message.audioUrl ? (
        <VoiceBubble url={message.audioUrl} durationSeconds={message.durationSeconds} />
      ) : (
        <Text style={{ color: colors.text }}>{message.text}</Text>
      )}
      <Text style={{ color: colors.muted, fontSize: 10, marginTop: 4, alignSelf: "flex-end" }}>
        {new Date(message.date).toLocaleTimeString("sr-RS", { hour: "2-digit", minute: "2-digit" })}
      </Text>
    </View>
  );
}

export default function ChatScreen() {
  const { chatId } = useLocalSearchParams<{ chatId: string }>();
  const { activeCommunityId, me, status: myStatus } = useCommunity();
  const [chat, setChat] = useState<ChatDetailsDto | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [text, setText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [recording, setRecording] = useState(false);
  const [showRewardPicker, setShowRewardPicker] = useState(false);
  const recorder = useAudioRecorder(RecordingPresets.HIGH_QUALITY);
  const listRef = useRef<FlatList<MessageDto>>(null);

  const loadMessages = useCallback(async () => {
    if (!activeCommunityId || !chatId) return;
    const all: MessageDto[] = [];
    let cursor: string | null = null;
    for (let i = 0; i < 10; i++) {
      const page = await messageApi.getPage(activeCommunityId, chatId, cursor);
      all.push(...page.items);
      cursor = page.nextCursor;
      if (!cursor) break;
    }
    setMessages(all);
  }, [activeCommunityId, chatId]);

  const loadChat = useCallback(async () => {
    if (!activeCommunityId || !chatId) return;
    try {
      setChat(await chatApi.get(activeCommunityId, chatId));
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId, chatId]);

  useEffect(() => {
    loadChat();
    loadMessages().catch((e) => setError(e.message));
    if (activeCommunityId && chatId) {
      messageApi.markRead(activeCommunityId, chatId).catch(() => {});
    }
  }, [loadChat, loadMessages]);

  useChannel(chatId ? `chat:${chatId}` : null, {
    "chat.message": (message: MessageDto) => {
      setMessages((current) =>
        current.some((m) => m.id === message.id) ? current : [...current, message]
      );
      if (activeCommunityId && chatId) messageApi.markRead(activeCommunityId, chatId).catch(() => {});
    },
    "chat.concluded": () => {
      loadChat();
    },
  });

  if (error && !chat) {
    return (
      <Screen>
        <Stack.Screen options={{ title: "Čet" }} />
        <ErrorText error={error} />
      </Screen>
    );
  }
  if (!chat || !me) return <Loading />;

  const others = chat.participants.filter((p) => p.membershipId !== me.membershipId);
  const title = others.map((p) => p.username).join(", ") || "Čet";
  const myRole = chat.participants.find((p) => p.membershipId === me.membershipId)?.role ?? null;
  const isHelp = chat.type === "HELP_REQUEST";
  const isTemporary = chat.type === "TEMPORARY";
  const helper = chat.participants.find((p) => p.role === "Helper");

  const sendText = async () => {
    if (!activeCommunityId || !text.trim()) return;
    setBusy(true);
    try {
      const sent = await messageApi.sendText(activeCommunityId, chat.id, text.trim());
      setMessages((current) =>
        current.some((m) => m.id === sent.id) ? current : [...current, sent]
      );
      setText("");
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const toggleRecording = async () => {
    if (!recording) {
      const permission = await AudioModule.requestRecordingPermissionsAsync();
      if (!permission.granted) return;
      await recorder.prepareToRecordAsync();
      recorder.record();
      setRecording(true);
      return;
    }
    setRecording(false);
    setBusy(true);
    try {
      const durationSeconds = Math.max(1, Math.round(recorder.currentTime));
      await recorder.stop();
      if (!recorder.uri || !activeCommunityId) return;
      const uploaded = await fileApi.uploadAudio({
        uri: recorder.uri,
        name: "voice.m4a",
        type: "audio/mp4",
      });
      const sent = await messageApi.sendVoice(activeCommunityId, chat.id, uploaded.url, durationSeconds);
      setMessages((current) =>
        current.some((m) => m.id === sent.id) ? current : [...current, sent]
      );
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    } finally {
      setBusy(false);
    }
  };

  const conclude = () => {
    Alert.alert("Zaključi saradnju", `Da li je ${helper?.username ?? "pomagač"} pomogao?`, [
      { text: "Odustani", style: "cancel" },
      {
        text: "Nije",
        onPress: async () => {
          try {
            setChat(await helpChatApi.concludeWithoutReward(activeCommunityId!, chat.id));
          } catch (e: any) {
            Alert.alert("Greška", e.message);
          }
        },
      },
      { text: "Jeste", onPress: () => setShowRewardPicker(true) },
    ]);
  };

  const reward = async (stars: number) => {
    setShowRewardPicker(false);
    try {
      setChat(await helpChatApi.concludeWithReward(activeCommunityId!, chat.id, stars));
    } catch (e: any) {
      Alert.alert("Greška", e.message);
    }
  };

  const resign = () => {
    Alert.alert("Odustajanje", "Da li sigurno odustaješ od pružanja pomoći?", [
      { text: "Ne", style: "cancel" },
      {
        text: "Odustani od pomaganja",
        style: "destructive",
        onPress: async () => {
          try {
            setChat(await helpChatApi.resign(activeCommunityId!, chat.id));
          } catch (e: any) {
            Alert.alert("Greška", e.message);
          }
        },
      },
    ]);
  };

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title }} />
      <KeyboardAvoidingView
        style={{ flex: 1 }}
        behavior={Platform.OS === "ios" ? "padding" : undefined}
      >
        {isHelp && (
          <View style={{ backgroundColor: colors.card, borderBottomWidth: 1, borderBottomColor: colors.border, padding: spacing.m }}>
            <View style={{ flexDirection: "row", alignItems: "center", flexWrap: "wrap", gap: spacing.s }}>
              <Text style={{ fontWeight: "700", color: colors.text }}>
                Ispomoć · {helpStatusLabel(chat.status)}
              </Text>
              {chat.awardedStars != null && (
                <Text style={{ color: colors.warning, fontWeight: "700" }}>⭐ +{chat.awardedStars}</Text>
              )}
              {chat.helpRequestId && (
                <Pressable
                  onPress={() =>
                    router.push({ pathname: "/post/[postId]", params: { postId: chat.helpRequestId! } })
                  }
                >
                  <Text style={{ color: colors.primary, fontSize: 12 }}>
                    Originalni {chat.helpRequestId}
                  </Text>
                </Pressable>
              )}
            </View>
            {chat.status === "Active" && myRole === "Requester" && !showRewardPicker && (
              <Button title="Zaključi saradnju" onPress={conclude} style={{ marginTop: spacing.s }} />
            )}
            {showRewardPicker && (
              <View style={{ marginTop: spacing.s }}>
                <Text style={{ color: colors.muted, marginBottom: spacing.s }}>
                  Odredi visinu nagrade:
                </Text>
                <View style={{ flexDirection: "row", gap: spacing.s }}>
                  {[50, 100, 150, 200, 250].map((stars) => (
                    <Pressable
                      key={stars}
                      onPress={() => reward(stars)}
                      style={{
                        flex: 1,
                        backgroundColor: colors.primary,
                        borderRadius: 8,
                        paddingVertical: spacing.s,
                        alignItems: "center",
                      }}
                    >
                      <Text style={{ color: "#fff", fontWeight: "700" }}>{stars}</Text>
                    </Pressable>
                  ))}
                </View>
              </View>
            )}
            {chat.status === "Active" && myRole === "Helper" && (
              <Button
                title="Odustani od pomaganja"
                variant="danger"
                onPress={resign}
                style={{ marginTop: spacing.s }}
              />
            )}
          </View>
        )}

        {isTemporary && (
          <View style={{ backgroundColor: colors.card, borderBottomWidth: 1, borderBottomColor: colors.border, padding: spacing.m }}>
            <Text style={{ color: colors.muted, marginBottom: spacing.s }}>
              Privremeni razgovor o potvrđivanju članstva.
            </Text>
            {myRole === "Issuer" ? (
              <Button title="Izdaj potvrdu (QR)" onPress={() => router.push("/certify-show")} />
            ) : (
              myStatus === "unconfirmed" && (
                <Button title="Skeniraj QR kod za potvrdu" onPress={() => router.push("/certify-scan")} />
              )
            )}
          </View>
        )}

        <FlatList
          ref={listRef}
          data={messages}
          keyExtractor={(m) => m.id}
          renderItem={({ item }) => (
            <MessageBubble message={item} mine={item.senderMembershipId === me.membershipId} />
          )}
          contentContainerStyle={{ padding: spacing.l }}
          onContentSizeChange={() => listRef.current?.scrollToEnd({ animated: false })}
        />

        {chat.canSend ? (
          <View
            style={{
              flexDirection: "row",
              alignItems: "center",
              padding: spacing.m,
              gap: spacing.s,
              backgroundColor: colors.card,
              borderTopWidth: 1,
              borderTopColor: colors.border,
            }}
          >
            <Pressable onPress={toggleRecording} hitSlop={8}>
              <Ionicons
                name={recording ? "stop-circle" : "mic-outline"}
                size={28}
                color={recording ? colors.danger : colors.primary}
              />
            </Pressable>
            <TextInput
              value={text}
              onChangeText={setText}
              placeholder={recording ? "Snimanje u toku..." : "Poruka..."}
              placeholderTextColor={colors.muted}
              editable={!recording}
              style={{
                flex: 1,
                backgroundColor: colors.background,
                borderRadius: 20,
                paddingHorizontal: spacing.m,
                paddingVertical: 8,
                color: colors.text,
              }}
            />
            <Button title="Pošalji" onPress={sendText} loading={busy} disabled={recording} />
          </View>
        ) : (
          <View
            style={{
              padding: spacing.m,
              backgroundColor: colors.card,
              borderTopWidth: 1,
              borderTopColor: colors.border,
              alignItems: "center",
            }}
          >
            <Text style={{ color: colors.muted }}>Slanje poruka u ovom četu više nije moguće.</Text>
          </View>
        )}
      </KeyboardAvoidingView>
    </Screen>
  );
}
