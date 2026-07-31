import { Text, View } from "react-native";
import { useCommunity } from "../state/CommunityContext";
import { formatDateTime } from "./labels";
import { colors, spacing } from "./theme";

export function MutedNotice() {
  const { isMuted, mutedUntil } = useCommunity();

  if (!isMuted || !mutedUntil) return null;

  return (
    <View
      style={{
        backgroundColor: colors.card,
        borderWidth: 1,
        borderColor: colors.warning,
        borderRadius: 8,
        padding: spacing.m,
        marginBottom: spacing.m,
      }}
    >
      <Text style={{ color: colors.warning, fontWeight: "600" }}>
        Utišani ste do {formatDateTime(mutedUntil)}.
      </Text>
      <Text style={{ color: colors.muted, marginTop: spacing.xs }}>
        Do tada ne možete da objavljujete, komentarišete, šaljete poruke ni da pokrećete namere.
        Glasanje o namerama vam ostaje.
      </Text>
    </View>
  );
}
