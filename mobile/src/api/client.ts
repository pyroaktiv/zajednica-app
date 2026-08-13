import AsyncStorage from "@react-native-async-storage/async-storage";
import { Directory, File, Paths } from "expo-file-system";
import type { AuthTokens } from "./types";

export const BASE_URL = process.env.EXPO_PUBLIC_API_URL!;

const STORAGE_KEY = "auth.tokens";

let tokens: AuthTokens | null = null;
let sessionExpiredListener: (() => void) | null = null;

export function onSessionExpired(listener: () => void) {
  sessionExpiredListener = listener;
}

export async function loadTokens(): Promise<AuthTokens | null> {
  const raw = await AsyncStorage.getItem(STORAGE_KEY);
  tokens = raw ? JSON.parse(raw) : null;
  return tokens;
}

export async function saveTokens(next: AuthTokens | null) {
  tokens = next;
  if (next) await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  else await AsyncStorage.removeItem(STORAGE_KEY);
}

export function currentTokens() {
  return tokens;
}

export function authorizedFile(path: string) {
  return {
    uri: `${BASE_URL}/${path}`,
    headers: tokens ? { Authorization: `Bearer ${tokens.accessToken}` } : undefined,
  };
}

export async function downloadAuthorizedFile(path: string): Promise<string> {
  const source = authorizedFile(path);
  const downloaded = await File.downloadFileAsync(source.uri, new Directory(Paths.cache), {
    headers: source.headers,
  });
  return downloaded.uri;
}

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

async function parseError(res: Response): Promise<ApiError> {
  let detail = `Greška (${res.status})`;
  try {
    const body = await res.json();
    if (body?.detail) detail = body.detail;
  } catch {}
  return new ApiError(res.status, detail);
}

async function refreshTokens(): Promise<boolean> {
  if (!tokens) return false;
  const res = await fetch(`${BASE_URL}/api/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken: tokens.refreshToken }),
  });
  if (!res.ok) return false;
  await saveTokens(await res.json());
  return true;
}

async function request<T>(path: string, init: RequestInit, retry = true): Promise<T> {
  const headers: Record<string, string> = { ...(init.headers as Record<string, string>) };
  if (tokens) headers.Authorization = `Bearer ${tokens.accessToken}`;

  const res = await fetch(`${BASE_URL}${path}`, { ...init, headers });

  if (res.status === 401 && tokens) {
    if (retry && (await refreshTokens())) return request<T>(path, init, false);
    await saveTokens(null);
    sessionExpiredListener?.();
    throw new ApiError(401, "Sesija je istekla.");
  }

  if (!res.ok) throw await parseError(res);
  if (res.status === 204) return null as T;
  const text = await res.text();
  return text ? JSON.parse(text) : (null as T);
}

function json(body: unknown): RequestInit {
  return {
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}

export const api = {
  get: <T>(path: string) => request<T>(path, { method: "GET" }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "POST", ...(body !== undefined ? json(body) : {}) }),
  put: <T>(path: string, body: unknown) => request<T>(path, { method: "PUT", ...json(body) }),
  delete: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "DELETE", ...(body !== undefined ? json(body) : {}) }),
  upload: <T>(path: string, file: { uri: string; name: string; type: string }) => {
    const form = new FormData();
    form.append("file", new File(file.uri) as unknown as Blob, file.name);
    return request<T>(path, { method: "POST", body: form });
  },
};
