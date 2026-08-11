import Constants from "expo-constants";
import * as Device from "expo-device";
import * as Notifications from "expo-notifications";
import { Platform } from "react-native";
import { deviceApi } from "../api/identity";

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: false,
  }),
});

let registeredToken: string | null = null;

const androidChannels = [
  { id: "emergency", name: "Hitni slučajevi", importance: Notifications.AndroidImportance.HIGH },
  { id: "management", name: "Upravnik", importance: Notifications.AndroidImportance.HIGH },
  { id: "chat", name: "Poruke", importance: Notifications.AndroidImportance.HIGH },
  { id: "posts", name: "Objave", importance: Notifications.AndroidImportance.DEFAULT },
];

export async function registerForPushNotifications() {
  console.warn("[push] start", { isDevice: Device.isDevice, alreadyRegistered: !!registeredToken });
  if (!Device.isDevice || registeredToken) return;

  if (Platform.OS === "android") {
    for (const channel of androidChannels) {
      await Notifications.setNotificationChannelAsync(channel.id, {
        name: channel.name,
        importance: channel.importance,
      });
    }
  }

  const existing = await Notifications.getPermissionsAsync();
  const granted =
    existing.status === "granted" || (await Notifications.requestPermissionsAsync()).status === "granted";
  console.warn("[push] permission", { granted });
  if (!granted) return;

  const projectId = Constants.expoConfig?.extra?.eas?.projectId;
  console.warn("[push] projectId", { projectId });
  const token = (await Notifications.getExpoPushTokenAsync(projectId ? { projectId } : undefined)).data;
  console.warn("[push] token", { token });

  await deviceApi.register(token);
  console.warn("[push] registered ok");
  registeredToken = token;
}

export async function unregisterForPushNotifications() {
  if (!registeredToken) return;
  try {
    await deviceApi.unregister(registeredToken);
  } catch {}
  registeredToken = null;
}
