import { api } from "./client";
import type { AuthTokens, ProfileDto, RegisterAccountRequest, UpdateProfileRequest } from "./types";

export const authApi = {
  register: (request: RegisterAccountRequest) => api.post<void>("/api/auth/register", request),
  verifyEmail: (token: string) => api.post<void>("/api/auth/verify-email", { token }),
  login: (usernameOrEmail: string, password: string) =>
    api.post<AuthTokens>("/api/auth/login", { usernameOrEmail, password }),
  logout: (refreshToken: string) => api.post<void>("/api/auth/logout", { refreshToken }),
};

export const deviceApi = {
  register: (token: string) => api.post<void>("/api/devices", { token }),
  unregister: (token: string) => api.delete<void>("/api/devices", { token }),
};

export const profileApi = {
  getMine: () => api.get<ProfileDto>("/api/profile/me"),
  update: (request: UpdateProfileRequest) => api.put<ProfileDto>("/api/profile", request),
  setImage: (key: string) => api.put<ProfileDto>("/api/profile/image", { key }),
  removeImage: () => api.delete<void>("/api/profile/image"),
};
