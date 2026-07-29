# Expo HAS CHANGED

Read the exact versioned docs at https://docs.expo.dev/versions/v57.0.0/ before writing any code.

# Struktura frontenda

- `src/api/` — `client.ts` (fetch + refresh tokena + upload), `types.ts` (DTO 1:1 sa backendom, camelCase),
  po jedan fajl po backend modulu (`identity`, `community`, `feed`, `chat`, `files`).
- `src/state/` — `AuthContext` (tokeni, login/logout), `CommunityContext` (aktivna zajednica, `me`,
  status `none|unconfirmed|confirmed`, uloge; osvežava se preko SignalR kanala zajednice).
- `src/realtime/connection.ts` — SignalR konekcija + `useChannel(kanal, handleri)` hook.
- `src/ui/` — `Basics.tsx` (Screen/Card/Button/Field/...), `theme.ts`, deljene kartice.
- `src/app/` — expo-router: `(auth)/` login/register/verify, `(tabs)/` 5 tabova sa gating-om po ulozi,
  ostale rute na root nivou (QR ekrani su fullscreen bez headera).
- Provera: `npx tsc --noEmit`. Nove rute traže regeneraciju typed routes: kratko `npx expo start`.
