import * as ImagePicker from "expo-image-picker";
import { router, Stack } from "expo-router";
import { useEffect, useState } from "react";
import { Image, ScrollView, Text } from "react-native";
import { fileApi } from "../api/files";
import { profileApi } from "../api/identity";
import { memberApi } from "../api/community";
import { useCommunity } from "../state/CommunityContext";
import { Button, ErrorText, Field, Screen } from "../ui/Basics";
import { colors, spacing } from "../ui/theme";

export default function ProfileEdit() {
  const { me, activeCommunityId, refresh } = useCommunity();
  const [firstName, setFirstName] = useState(me?.firstName ?? "");
  const [lastName, setLastName] = useState(me?.lastName ?? "");
  const [phone, setPhone] = useState(me?.phone ?? "");
  const [contactEmail, setContactEmail] = useState(me?.contactEmail ?? "");
  const [unitNumber, setUnitNumber] = useState(me?.unitNumber ?? "");
  const [imageUrl, setImageUrl] = useState(me?.imageUrl ?? null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (me) return;
    profileApi
      .getMine()
      .then((p) => {
        setFirstName(p.firstName ?? "");
        setLastName(p.lastName ?? "");
        setPhone(p.phone ?? "");
        setContactEmail(p.contactEmail ?? "");
        setImageUrl(p.imageUrl);
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
      setImageUrl(uploaded.url);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
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
        imageUrl,
      });
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
        {imageUrl && (
          <Image
            source={{ uri: imageUrl }}
            style={{ width: 88, height: 88, borderRadius: 44, alignSelf: "center", marginBottom: spacing.m }}
          />
        )}
        <Button title="Promeni profilnu sliku" variant="secondary" onPress={pickImage} />
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
