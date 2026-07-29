import { router } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";
import type { PostDto } from "../api/types";
import { formatDateTime, postKindLabel } from "./labels";
import { colors, spacing } from "./theme";

export function PostCard({ post }: { post: PostDto }) {
  const kind = postKindLabel(post.kind);
  const isHelp = post.type === "HELP_REQUEST";

  return (
    <Pressable
      style={styles.card}
      onPress={() => router.push({ pathname: "/post/[postId]", params: { postId: post.id } })}
    >
      <View style={{ flexDirection: "row", alignItems: "center", marginBottom: spacing.s }}>
        {post.authorImageUrl ? (
          <Image source={{ uri: post.authorImageUrl }} style={styles.avatar} />
        ) : (
          <View style={[styles.avatar, styles.avatarFallback]}>
            <Text style={{ color: colors.muted, fontSize: 12, fontWeight: "700" }}>
              {post.authorUsername.slice(0, 1).toUpperCase()}
            </Text>
          </View>
        )}
        <View style={{ flex: 1, marginLeft: spacing.s }}>
          <Text style={{ fontWeight: "600", color: colors.text }}>{post.authorUsername}</Text>
          <Text style={{ color: colors.muted, fontSize: 11 }}>{formatDateTime(post.dateCreated)}</Text>
        </View>
        {kind && <Text style={[styles.badge, { color: kind.color, borderColor: kind.color }]}>{kind.text}</Text>}
        {isHelp && (
          <Text style={[styles.badge, { color: colors.primary, borderColor: colors.primary }]}>
            {post.closed ? "Ispomoć · zatvorena" : "Ispomoć"}
          </Text>
        )}
      </View>
      <Text style={{ color: colors.text }} numberOfLines={4}>
        {post.text}
      </Text>
      {post.imageUrls.length > 0 && (
        <Text style={{ color: colors.muted, fontSize: 12, marginTop: spacing.s }}>
          📷 {post.imageUrls.length} {post.imageUrls.length === 1 ? "slika" : "slike"}
        </Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.card,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.m,
    marginBottom: spacing.s,
  },
  avatar: {
    width: 32,
    height: 32,
    borderRadius: 16,
  },
  avatarFallback: {
    backgroundColor: colors.border,
    alignItems: "center",
    justifyContent: "center",
  },
  badge: {
    borderWidth: 1,
    borderRadius: 6,
    paddingHorizontal: 6,
    paddingVertical: 2,
    fontSize: 11,
    fontWeight: "700",
    marginLeft: spacing.xs,
  },
});
