import { Image } from "expo-image";
import * as ImagePicker from "expo-image-picker";
import { router, Stack } from "expo-router";
import { useEffect, useState } from "react";
import { ScrollView, Text } from "react-native";
import { authorizedFile } from "../api/client";
import { fileApi } from "../api/files";
import { profileApi } from "../api/identity";
import { memberApi } from "../api/community";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText, Field, Screen } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

type ImageSource = { uri: string; headers?: Record<string, string> };
type ImageChange = { set: string } | "remove" | null;

export default function ProfileEdit() {
  const { me, activeCommunityId, refresh } = useCommunity();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phone, setPhone] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [unitNumber, setUnitNumber] = useState(me?.unitNumber ?? "");
  const [imagePreview, setImagePreview] = useState<ImageSource | null>(null);
  const [imageChange, setImageChange] = useState<ImageChange>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    profileApi
      .getMine()
      .then((p) => {
        setFirstName(p.firstName ?? "");
        setLastName(p.lastName ?? "");
        setPhone(p.phone ?? "");
        setContactEmail(p.contactEmail ?? "");
        setImagePreview(p.imageUrl ? authorizedFile(p.imageUrl) : null);
      })
      .catch((e) => setError(e.message));
  }, []);

  const pickImage = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({ mediaTypes: "images", quality: 0.7 });
    if (result.canceled) return;
    const asset = result.assets[0];
    setBusy(true);
    try {
      const uploaded = await fileApi.uploadImage({
        uri: asset.uri,
        name: asset.fileName ?? "profile.jpg",
        type: asset.mimeType ?? "image/jpeg",
      });
      setImageChange({ set: uploaded.key });
      setImagePreview({ uri: asset.uri });
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  };

  const removeImage = () => {
    setImageChange("remove");
    setImagePreview(null);
  };

  const submit = async () => {
    setError(null);
    setBusy(true);
    try {
      await profileApi.update({
        firstName: firstName.trim() || null,
        lastName: lastName.trim() || null,
        phone: phone.trim() || null,
        contactEmail: contactEmail.trim() || null,
      });
      if (imageChange === "remove") await profileApi.removeImage();
      else if (imageChange) await profileApi.setImage(imageChange.set);
      if (me && activeCommunityId) {
        await memberApi.setUnitNumber(activeCommunityId, unitNumber.trim() || null);
      }
      await refresh();
      router.back();
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Screen style={{ padding: 0 }}>
      <Stack.Screen options={{ title: "Izmena profila" }} />
      <ScrollView contentContainerStyle={{ padding: spacing.l }}>
        {imagePreview && (
          <Image
            source={imagePreview}
            style={{ width: 88, height: 88, borderRadius: 44, alignSelf: "center", marginBottom: spacing.m }}
          />
        )}
        <Button title="Promeni profilnu sliku" variant="secondary" onPress={pickImage} />
        {imagePreview && (
          <Button title="Ukloni sliku" variant="danger" onPress={removeImage} style={{ marginTop: spacing.s }} />
        )}
        <Text style={{ marginTop: spacing.l }} />
        <Field label="Ime" value={firstName} onChangeText={setFirstName} />
        <Field label="Prezime" value={lastName} onChangeText={setLastName} />
        <Field label="Telefon" value={phone} onChangeText={setPhone} keyboardType="phone-pad" />
        <Field
          label="Kontakt mejl"
          value={contactEmail}
          onChangeText={setContactEmail}
          autoCapitalize="none"
          keyboardType="email-address"
        />
        {me && (
          <Field label="Broj posebnog dela" value={unitNumber} onChangeText={setUnitNumber} />
        )}
        <ErrorText error={error} />
        <Button title="Sačuvaj" onPress={submit} loading={busy} />
        <Text style={{ color: colors.muted, marginTop: spacing.m, fontSize: 12 }}>
          Lični podaci važe za ceo nalog; broj posebnog dela važi samo u izabranoj zajednici.
        </Text>
      </ScrollView>
    </Screen>
  );
}
