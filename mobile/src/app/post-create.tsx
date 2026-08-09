import * as ImagePicker from "expo-image-picker";
import { router, Stack } from "expo-router";
import { useState } from "react";
import { Image, Pressable, ScrollView, Text, type TextStyle, TextInput, View } from "react-native";
import { postApi } from "../api/feed";
import { fileApi } from "../api/files";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText, Screen, SectionTitle } from "../ui/Basics";
import { MutedNotice } from "../ui/MutedNotice";
import { colors, spacing } from "../ui/theme";

type PostType = "general" | "help";
type Kind = "Plain" | "Problem" | "Emergency";

function Option({
  label,
  selected,
  onPress,
  labelStyle,
}: {
  label: string;
  selected: boolean;
  onPress: () => void;
  labelStyle?: TextStyle;
}) {
  return (
    <Pressable
      onPress={onPress}
      style={{
        flexDirection: "row",
        alignItems: "center",
        paddingVertical: spacing.s,
        gap: spacing.s,
      }}
    >
      <View
        style={{
          width: 20,
          height: 20,
          borderRadius: 10,
          borderWidth: 2,
          borderColor: selected ? colors.primary : colors.border,
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        {selected && (
          <View style={{ width: 10, height: 10, borderRadius: 5, backgroundColor: colors.primary }} />
        )}
      </View>
      <Text style={[{ color: colors.text }, labelStyle]}>{label}</Text>
    </Pressable>
  );
}

function placeholderFor(type: PostType, kind: Kind) {
  if (type === "help") return "Kako komšije mogu da pomognu?";
  if (kind === "Problem") return "Opis problema...";
  if (kind === "Emergency") return "Opis hitnog slučaja...";
  return "Šta ima novo u zgradi?";
}

export default function PostCreate() {
  const { activeCommunityId, isMuted } = useCommunity();
  const [type, setType] = useState<PostType>("general");
  const [kind, setKind] = useState<Kind>("Plain");
  const [text, setText] = useState("");
  const [imageUrls, setImageUrls] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const addImage = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({ mediaTypes: "images", quality: 0.7 });
    if (result.canceled) return;
    const asset = result.assets[0];
    setBusy(true);
    try {
      const uploaded = await fileApi.uploadImage({
        uri: asset.uri,
        name: asset.fileName ?? "image.jpg",
        type: asset.mimeType ?? "image/jpeg",
      });
      setImageUrls((current) => [...current, uploaded.url]);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  };

  const submit = async () => {
    if (!activeCommunityId) return;
    setError(null);
    setBusy(true);
    try {
      if (type === "general") await postApi.createGeneral(activeCommunityId, text.trim(), kind, imageUrls);
      else await postApi.createHelpRequest(activeCommunityId, text.trim(), imageUrls);
      router.back();
    } catch (e: any) {
      setError(e.message);
      setBusy(false);
    }
  };

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: "Nova objava" }} />
      <ScrollView contentContainerStyle={{ padding: spacing.l }}>
        <MutedNotice />
        <SectionTitle>Tip objave</SectionTitle>
        <Option label="Opšta objava" selected={type === "general"} onPress={() => setType("general")} />
        <Option label="Komšijska ispomoć" selected={type === "help"} onPress={() => setType("help")} />

        {type === "general" && (
          <>
            <SectionTitle>Kategorija</SectionTitle>
            <Option label="Obična objava" selected={kind === "Plain"} onPress={() => setKind("Plain")} />
            <Option
              label="U ovoj objavi je opisan problem koji treba rešiti"
              selected={kind === "Problem"}
              onPress={() => setKind("Problem")}
            />
            <Option
              label="Potrebna je hitna reakcija zajednice"
              selected={kind === "Emergency"}
              onPress={() => setKind("Emergency")}
              labelStyle={{ color: colors.danger, fontWeight: "700" }}
            />
          </>
        )}

        <SectionTitle>Tekst</SectionTitle>
        <TextInput
          multiline
          value={text}
          onChangeText={setText}
          placeholder={placeholderFor(type, kind)}
          placeholderTextColor={colors.muted}
          style={{
            backgroundColor: colors.card,
            borderWidth: 1,
            borderColor: colors.border,
            borderRadius: 8,
            padding: spacing.m,
            minHeight: 120,
            textAlignVertical: "top",
            color: colors.text,
            marginBottom: spacing.m,
          }}
        />

        {imageUrls.length > 0 && (
          <View style={{ flexDirection: "row", flexWrap: "wrap", gap: spacing.s, marginBottom: spacing.m }}>
            {imageUrls.map((url) => (
              <Image key={url} source={{ uri: url }} style={{ width: 72, height: 72, borderRadius: 8 }} />
            ))}
          </View>
        )}
        <Button title="Dodaj sliku" variant="secondary" onPress={addImage} loading={busy} />
        <ErrorText error={error} />
        {!isMuted && (
          <Button title="Objavi" onPress={submit} loading={busy} style={{ marginTop: spacing.l }} />
        )}
      </ScrollView>
    </Screen>
  );
}
