# zajednica.app

A mobile application for residential building communities.

- `backend/` — a modular monolith (.NET 10, PostgreSQL).
- `mobile/` — the client (Expo SDK 57, React Native).

The system has three parts. Start each part separately:

1. **PostgreSQL 17** in Docker.
2. **The backend** (`Zajednica.Api`), on the host at `0.0.0.0:5265`.
3. **The mobile client**, on a phone or an emulator.

## Where each part is

| Part | Location |
|---|---|
| Backend solution | `backend/Zajednica.slnx` |
| Backend host process | `backend/src/Zajednica.Api/` |
| Backend modules | `backend/src/Modules/` (`Identity`, `Community`, `Feed`, `Chat`) |
| Local Database container | `backend/docker-compose.yml` (PostgreSQL 17) |
| Backend scripts | `backend/scripts/` (`reset-db.sh`, `sync-ip.sh`, `dev-before.sh`) |
| Mobile client code | `mobile/src/` (`app`, `api`, `realtime`, `notifications`, `state`, `ui`) |
| Mobile app identity | `mobile/app.json` |

## Where each configuration resides

| Setting | File | Note |
|---|---|---|
| Database credentials | `backend/.env` | Copy from `.env.example`. Docker and the scripts read this file. |
| Connection string, JWT key | .NET user-secrets (`backend/src/Zajednica.Api/`) |
| Default backend settings | `backend/src/Zajednica.Api/appsettings.json` |
| Development overrides | `backend/src/Zajednica.Api/appsettings.Development.json` | Enables seeding and sets default local DB strings. |
| Backend API URL for the client | `mobile/.env` | Set `EXPO_PUBLIC_API_URL` to `http://<LAN_IP>:5265`. |

## Backend settings

`appsettings.json` holds the defaults and documents all the sections (`Jwt`,
`Auth`, `Storage`, `Smtp`, `Cors`, `Push`, `Seed`). `appsettings.Development.json`
overrides some of them. User-secrets override both.

| Key | Where you set it | Note |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | user-secrets | The PostgreSQL connection string. |
| `Jwt:Key` | user-secrets | The token signing key. Minimum 32 bytes. |
| `Smtp:Enabled` | `appsettings.json` | If kept `false`, the activation code shows in the console as `[Email:DEV]`. |
| `Seed:Enabled` | `appsettings.Development.json` | Already `true`. The backend makes the test accounts when the database is empty. See DevDataSeeder.cs for details. |

## What the scripts do

The scripts in `backend/scripts/` read `backend/.env` for the database
credentials.

| Script | Action |
|---|---|
| `sync-ip.sh` | Finds the LAN IP of the host and writes `EXPO_PUBLIC_API_URL=http://<IP>:5265` into `mobile/.env`, so the client always finds the backend. Run this before starting the local Metro dev server.|
| `dev-before.sh` | Starts PostgreSQL, waits until the database is ready, then runs `sync-ip.sh`. Run it before the backend. |
| `reset-db.sh [Module ...]` | Drops each module schema and builds it again from the committed migrations. With no argument it does all four modules. This deletes the data in those schemas. |

## Quick local start

```bash
# 1. Database
cd backend
cp .env.example .env          # the first time only
docker compose up -d          # PostgreSQL 17 at localhost:5432

# 2. Backend configuration (the first time only)
cd backend/src/Zajednica.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=zajednica;Username=zajednica;Password=devpassword"
dotnet user-secrets set "Jwt:Key" "<your-32-byte-signing-key>"

# 3. Initialize empty database schemas (from the migrations)
cd backend
./scripts/reset-db.sh

# 4. Backend
dotnet run -lp http --project src/Zajednica.Api        # listens on 0.0.0.0:5265

# 5. Mobile client
cd ../mobile
npm install                   # the first time only
npx expo start --clear        # start the Expo Go 
```

## EAS build

```bash
cd mobile
eas build -p android --profile development  # internal APK (dev client shell, Metro dev server)
eas build -p android --profile preview      # internal APK
eas build -p android --profile production   # AAB for the Play Store
```

The project is tied to an Expo account through `extra.eas.projectId` in `mobile/app.json`.
If you build under your own account, run `eas init --force` to bind it to your project.

## Push notifications (Firebase) and google-services.json

Notifications go through the Expo Push service, which on Android delivers through FCM. Two
things from your own Firebase project are required:

1. **`google-services.json`** — from the Firebase console (an Android app with the package
   `app.zajednica`); drop it into the `mobile/` root.
2. **FCM admin JSON** (service account key, FCM V1) — upload it to Expo (`eas credentials`
   or the Expo dashboard) so Expo can send push to your FCM.

**If you have no Firebase and the build fails on `google-services.json`:** remove the line
`"googleServicesFile": "./google-services.json"` from the `android` block in `mobile/app.json`
and the build passes without push notifications.
