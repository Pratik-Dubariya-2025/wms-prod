# WMS — Workspace Management System

A workspace management platform with fine-grained, policy-based access control (PBAC): HR, projects, tasks, time logs, CRM, leaves, and invoicing.

- **Backend:** .NET 9 Web API (Clean Architecture, EF Core, MediatR/CQRS), SQL Server 2022, Redis (permission cache)
- **Frontend:** React 19 + TypeScript + Vite + Tailwind CSS v4 (light/dark themes)

---

## Prerequisites

| Tool | Version | Needed for |
| ---- | ------- | ---------- |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | latest | running the full stack |
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0+ | running the backend on the host |
| [Node.js](https://nodejs.org/) | 22.12+ | running the frontend on the host |

---

## Run everything with Docker

```bash
cp .env.example .env   # fill in the values — see below
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

`docker-compose.local.yml` adds host port mappings (`DB_PORT`, `API_PORT`, `WEB_PORT` from `.env`) on top of the base `docker-compose.yml`, which alone doesn't expose any host ports. With the example `.env` values:

| Service | URL |
| ------- | --- |
| Frontend | http://localhost:5173 |
| API / Swagger | http://localhost:5080/swagger |
| SQL Server | `localhost:1433` |

- EF Core migrations apply automatically on API startup (`Program.cs` calls `Database.Migrate()`), seeding the initial modules/permissions/roles.
- Redis and SQL Server passwords are **required** env vars with no fallback default — `docker compose up` fails fast with a clear error if `REDIS_PASSWORD` or `MSSQL_SA_PASSWORD` aren't set in `.env`, rather than silently using a hardcoded password.
- Data persists in named volumes (`wms-mssql-data`, `wms-redis-data`, `wms-dataprotection-keys`). `docker compose down -v` wipes them.

## Run on the host (faster for development)

```bash
docker compose up -d db cache   # just the two stateful dependencies

cd Backend
dotnet ef database update --project WMS.Infrastructure --startup-project WMS.API
dotnet run --project WMS.API --launch-profile http
```

Then, in a new terminal:

```bash
cd frontend
npm install
echo "VITE_API_BASE_URL=http://localhost:5247/api" > .env.local   # git-ignored
npm run dev
```

## Configuration

See `.env.example` for the full list. The important ones:

- `CONNECTION_STRING`, `REDIS_PASSWORD` — no defaults, must be set.
- `JWTSETTINGS_SECRET` — generate your own strong random value, don't reuse the one in `Backend/WMS.API/appsettings.json` (that file is a local-dev template with obvious placeholder text, not real credentials, and is itself gitignored).
- `ALLOWED_ORIGIN` — the deployed frontend's origin; CORS rejects everything else.
- `SMTP_*` — used for invite and password-reset emails.
- `DataProtection__KeysPath` (set to `/keys` in `docker-compose.yml`, backed by the `wms-dataprotection-keys` volume) — where encrypted MFA secrets' key ring is persisted. **This must survive container restarts** or every already-enrolled user's MFA becomes undecryptable.

## Default login

| Email | Password |
| ----- | -------- |
| `admin@wms.com` | `Admin@123` |

This is a seeded bootstrap account (`InitialRBACSchema` migration) with `IsFirstTimeLogin` set — logging in with it doesn't grant a session, it routes into the forced first-time-login flow (verify email → set a new password) before any token is issued. Still, change this password (or replace the seed) before a real deployment goes live with real data.

---

## Production-Readiness Notes

This repo went through a security/prod-readiness pass. Fixed:

- **Real secrets were committed to git.** `Backend/WMS.API/appsettings.json` had a live SMTP mailbox password, a JWT signing secret, and a Redis password committed (the `.gitignore` entry excluding it was added after the first commit, so it never actually took effect in the old repo's history). This repo has fresh history — nothing here was ever committed with real secrets in it. The local `appsettings.json` now has only placeholders; real values are supplied via environment variables (see `.env.example`).
- **`docker-compose.yml` had the same Redis password hardcoded as a fallback default** (`${REDIS_PASSWORD:-WmsRedisSecretPass123!}`) in three places, meaning even the "production" compose file would silently run with a known password if the env var wasn't set. Changed to `${REDIS_PASSWORD:?...}`, which fails the `docker compose up` outright with a clear error instead of silently falling back to something insecure.
- **MFA/TOTP secrets were stored in plaintext** in the database (`user.MfaSecret = secret` with no encryption). Anyone with DB read access — including via an unrelated SQL injection elsewhere, or a DB backup left somewhere it shouldn't be — could extract every enrolled user's TOTP seed and generate valid codes, defeating the second factor entirely. Fixed with a new `IMfaSecretProtector` wrapping ASP.NET Core's Data Protection API: secrets are encrypted before being written to `SetupMfaCommand`, and decrypted for verification in `LoginCommand`/`VerifyMfaCommand`. The key ring is persisted to a mounted volume (`DataProtection:KeysPath`) so it survives container restarts — without that, the encrypted secrets would become permanently undecryptable the moment the key ring regenerated, locking out every MFA-enrolled user. **Found via actually running the container, not just writing the code**: the non-root Docker user (see below) initially couldn't write to that volume at all — `System.UnauthorizedAccessException: Access to the path '/keys/...' is denied` — because Docker named volumes are root-owned by default. Fixed by creating and `chown`ing `/keys` in the Dockerfile before switching to the non-root user, so Docker's volume-initialization (which copies the image directory's ownership into a fresh named volume on first mount) gets it right. Re-tested from a clean volume afterward to confirm.
- **CORS hardcoded to `http://localhost:5173`** — now reads `AllowedOrigins` from config (`AllowedOrigins__0` in the Docker environment), same pattern as the other two.
- **Backend Docker image ran as root** — added `USER $APP_UID` (with the `/keys` ownership fix above so it doesn't just move the problem).
- **No `.env.example`** existed, despite `docker-compose.yml` requiring `CONNECTION_STRING`, `JWTSETTINGS_*`, and (now) `REDIS_PASSWORD` with no defaults — `docker compose up` as the old README documented it would not have worked out of the box. Added one covering every variable both compose files reference.
- **README documentation drift**: claimed a specific SQL Server SA password (`Dha@17357`) lived in `appsettings.json`/`docker-compose.yml` — it didn't exist in either file, and running the documented `docker compose up --build` alone wouldn't have exposed the ports the README claimed either (`docker-compose.yml` alone maps no host ports; that's `docker-compose.local.yml`'s job). Rewritten to match what's actually in the repo, verified by actually running it.
- Fixed a handful of real lint findings while going through the frontend: several `catch` blocks that silently swallowed errors with no logging at all (now at least `console.error`'d), and an ESLint config gap flagging an intentional destructure-and-discard pattern (`const { items: _, ...rest } = data`) used identically across seven feature hooks as if it were dead code.

Verified, not just written: both the .NET backend and the React frontend build cleanly; ran the **full stack via `docker compose up --build`** (SQL Server, Redis, API, web) from a clean state — confirmed all 26 EF Core migrations apply successfully, the Data Protection key ring writes successfully under the new non-root user, CORS responds correctly to a real preflight request against the configured origin, and the frontend's nginx reverse-proxy to the API works.

Still open (tracked, not silently ignored):
- The frontend has ~65 pre-existing lint errors, mostly `@typescript-eslint/no-explicit-any` (40) and a newer `react-hooks/set-state-in-effect` rule flagging 23 effects that call `setState` synchronously in their body (a real anti-pattern — can cause cascading renders — but each one needs individual judgment about whether it belongs in an event handler instead, too large/risky to blanket-fix here).
- `npm audit` reports vulnerabilities in dependencies — worth a dedicated pass with its own testing.
- No structured production logging (Serilog/App Insights/Seq) — only default `ILogger` console output.
- The full MFA setup → verify → login round trip wasn't exercised via a live HTTP flow in this pass (this app has no public self-registration endpoint — users are invite-based — so bootstrapping a test user would need a working invite chain through an already-authenticated admin session). What *was* verified directly: the Data Protection wiring resolves via DI, the key ring persists and is writable under the real non-root container, and `Protect`/`Unprotect` go through the exact same single protector instance for both the write path (`SetupMfaCommand`) and both read paths (`LoginCommand`, `VerifyMfaCommand`) by construction.
