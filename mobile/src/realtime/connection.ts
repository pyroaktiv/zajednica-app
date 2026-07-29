import * as signalR from "@microsoft/signalr";
import { useEffect } from "react";
import { BASE_URL, currentTokens } from "../api/client";

let connection: signalR.HubConnection | null = null;
const joinedChannels = new Set<string>();

export function startRealtime() {
  if (connection) return;
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/realtime`, {
      accessTokenFactory: () => currentTokens()?.accessToken ?? "",
    })
    .withAutomaticReconnect()
    .build();
  connection.onreconnected(() => {
    for (const channel of joinedChannels) connection?.invoke("JoinChannel", channel).catch(() => {});
  });
  connection.start().catch(() => {});
}

export function stopRealtime() {
  joinedChannels.clear();
  connection?.stop().catch(() => {});
  connection = null;
}

function whenConnected(action: (c: signalR.HubConnection) => void) {
  if (!connection) return;
  if (connection.state === signalR.HubConnectionState.Connected) action(connection);
  else setTimeout(() => whenConnected(action), 500);
}

export function useChannel(channel: string | null, handlers: Record<string, (payload: any) => void>) {
  useEffect(() => {
    if (!channel || !connection) return;
    const c = connection;
    joinedChannels.add(channel);
    whenConnected((conn) => conn.invoke("JoinChannel", channel).catch(() => {}));
    for (const [event, handler] of Object.entries(handlers)) c.on(event, handler);
    return () => {
      joinedChannels.delete(channel);
      for (const [event, handler] of Object.entries(handlers)) c.off(event, handler);
      whenConnected((conn) => conn.invoke("LeaveChannel", channel).catch(() => {}));
    };
  }, [channel]);
}
