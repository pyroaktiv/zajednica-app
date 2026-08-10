import { createContext, ReactNode, useContext, useEffect, useState } from "react";
import { currentTokens, loadTokens, onSessionExpired, saveTokens } from "../api/client";
import { authApi } from "../api/identity";
import { registerForPushNotifications, unregisterForPushNotifications } from "../notifications/push";
import { startRealtime, stopRealtime } from "../realtime/connection";

type AuthStatus = "loading" | "signedOut" | "signedIn";

type AuthContextValue = {
  status: AuthStatus;
  login: (usernameOrEmail: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue>(null!);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>("loading");

  useEffect(() => {
    onSessionExpired(() => {
      stopRealtime();
      setStatus("signedOut");
    });
    loadTokens().then((tokens) => {
      if (tokens) {
        startRealtime();
        registerForPushNotifications().catch(() => {});
      }
      setStatus(tokens ? "signedIn" : "signedOut");
    });
  }, []);

  const login = async (usernameOrEmail: string, password: string) => {
    const tokens = await authApi.login(usernameOrEmail, password);
    await saveTokens(tokens);
    startRealtime();
    registerForPushNotifications().catch(() => {});
    setStatus("signedIn");
  };

  const logout = async () => {
    const tokens = currentTokens();
    await unregisterForPushNotifications();
    if (tokens) {
      try {
        await authApi.logout(tokens.refreshToken);
      } catch {}
    }
    await saveTokens(null);
    stopRealtime();
    setStatus("signedOut");
  };

  return <AuthContext.Provider value={{ status, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
