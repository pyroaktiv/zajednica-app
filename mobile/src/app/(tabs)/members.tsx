import { useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import { FlatList, Pressable, Text, View } from "react-native";
import { memberApi } from "../../api/community";
import type { MemberSummaryDto } from "../../api/types";
import { useCommunity } from "../../state/CommunityContext";
import { EmptyState, ErrorText, Screen } from "../../ui/Basics";
import { MemberRow } from "../../ui/MemberRow";
import { CertificationShortcut, JoinCommunityShortcut } from "../../ui/Shortcuts";
import { colors, spacing } from "../../ui/theme";

type Segment = "confirmed" | "unconfirmed";

export default function Members() {
  const { activeCommunityId, isIssuer, status } = useCommunity();
  const [segment, setSegment] = useState<Segment>("confirmed");
  const [members, setMembers] = useState<MemberSummaryDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!activeCommunityId || status !== "confirmed") return;
    setError(null);
    try {
      setMembers(
        segment === "confirmed"
          ? await memberApi.getConfirmed(activeCommunityId)
          : await memberApi.getUnconfirmed(activeCommunityId)
      );
    } catch (e: any) {
      setError(e.message);
    }
  }, [activeCommunityId, segment, status]);

  useFocusEffect(
    useCallback(() => {
      load();
    }, [load])
  );

  if (status === "none") return <JoinCommunityShortcut />;
  if (status === "unconfirmed") return <CertificationShortcut />;

  return (
    <Screen>
      {isIssuer && (
        <View style={{ flexDirection: "row", marginBottom: spacing.m, gap: spacing.s }}>
          {(["confirmed", "unconfirmed"] as Segment[]).map((value) => (
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
                {value === "confirmed" ? "Potvrđeni" : "Nepotvrđeni"}
              </Text>
            </Pressable>
          ))}
        </View>
      )}
      <ErrorText error={error} />
      <FlatList
        data={members}
        keyExtractor={(m) => m.membershipId}
        renderItem={({ item }) => <MemberRow member={item} />}
        ListEmptyComponent={<EmptyState text="Nema članova za prikaz." />}
      />
    </Screen>
  );
}
