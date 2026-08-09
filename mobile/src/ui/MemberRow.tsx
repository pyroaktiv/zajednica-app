import { router } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";
import type { MemberSummaryDto } from "../api/types";
import { RoleNames } from "../api/types";
import { colors, spacing } from "./theme";

export function roleLabel(role: string) {
  if (role === RoleNames.Issuer) return "Izdavač potvrda";
  if (role === RoleNames.Manager) return "Upravnik";
  return role;
}

export function MemberRow({ member }: { member: MemberSummaryDto }) {
  return (
    <Pressable
      style={styles.row}
      onPress={() => router.push({ pathname: "/member/[membershipId]", params: { membershipId: member.membershipId } })}
    >
      {member.imageUrl ? (
        <Image source={{ uri: member.imageUrl }} style={styles.avatar} />
      ) : (
        <View style={[styles.avatar, styles.avatarFallback]}>
          <Text style={{ color: colors.muted, fontWeight: "700" }}>
            {member.username.slice(0, 1).toUpperCase()}
          </Text>
        </View>
      )}
      <View style={{ flex: 1, marginLeft: spacing.m }}>
        <Text style={{ fontWeight: "600", color: colors.text }}>{member.username}</Text>
        {member.roles.length > 0 && (
          <View style={{ flexDirection: "row", gap: spacing.xs, marginTop: 2 }}>
            {member.roles.map((role) => (
              <Text key={role} style={styles.badge}>
                {roleLabel(role)}
              </Text>
            ))}
          </View>
        )}
      </View>
      {member.isConfirmed && <Text style={{ color: colors.warning }}>⭐ {member.stars ?? 0}</Text>}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: colors.card,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.m,
    marginBottom: spacing.s,
  },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: 20,
  },
  avatarFallback: {
    backgroundColor: colors.border,
    alignItems: "center",
    justifyContent: "center",
  },
  badge: {
    backgroundColor: "#e8effc",
    color: colors.primaryDark,
    borderRadius: 6,
    paddingHorizontal: 6,
    paddingVertical: 1,
    fontSize: 11,
    overflow: "hidden",
  },
});
