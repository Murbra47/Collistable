# Collistable

A full-stack game collection tracker. Track the games you own and want — with data pulled directly from the IGDB database.

**Live demo:** https://collistable.netlify.app

---

## Features

- **Google SSO** — sign in with your Google account, no password required
- **Game search** — search the IGDB database and import games directly into your collection
- **Collection tracking** — mark games as owned or on your wishlist
- **User-scoped data** — each user sees only their own collection

---

## Tech Stack

**Frontend**
- Vue 3 + TypeScript
- Vite
- Pinia (state management)
- TanStack Query (server state / caching)
- Vue Router
- Bootstrap 5 + SCSS
- vue3-google-login

**Backend**
- .NET Core 8 (ASP.NET Core Web API)
- Entity Framework Core 8
- JWT bearer authentication
- Google.Apis.Auth (Google ID token validation)
- SQL Server (local) / PostgreSQL (production)
- Swagger / OpenAPI

**Infrastructure**
- Railway (API + PostgreSQL)
- Netlify (frontend)
- GitHub Actions (CI)

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/google` | No | Exchange Google ID token for app JWT |
| GET | `/api/games` | Yes | Get all games for the current user |
| GET | `/api/games/{id}` | Yes | Get a specific game |
| POST | `/api/games` | Yes | Create a game |
| PUT | `/api/games/{id}` | Yes | Update a game |
| DELETE | `/api/games/{id}` | Yes | Delete a game |
| POST | `/api/games/import` | Yes | Import a game from IGDB |
| GET | `/api/igdb/search?q={query}` | No | Search the IGDB database |

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- SQL Server (local instance or Docker)
- Node.js 18+
- A Google Cloud project with OAuth 2.0 credentials
- A Twitch developer account (required for IGDB API access)

### Backend

```bash
cd backend/Collistable.Api
cp .env.example .env
# Fill in your secrets in .env
dotnet run
```

The API will be available at `http://localhost:5053`.
Swagger UI is available at `http://localhost:5053/swagger` in development.

### Frontend

```bash
cd frontend/collistable-frontend
cp .env.example .env.local
# Fill in your secrets in .env.local
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`.

### Environment Variables

**Backend (`.env`)**

| Variable | Description |
|----------|-------------|
| `JWT__SECRET` | Secret key for signing JWTs (min 32 chars) |
| `JWT__ISSUER` | JWT issuer (e.g. `Collistable`) |
| `JWT__AUDIENCE` | JWT audience (e.g. `Collistable`) |
| `GOOGLE__CLIENTID` | Google OAuth client ID |
| `TWITCH_CLIENT_ID` | Twitch app client ID (for IGDB) |
| `TWITCH_CLIENT_SECRET` | Twitch app client secret (for IGDB) |

**Frontend (`.env.local`)**

| Variable | Description |
|----------|-------------|
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID |
| `VITE_API_BASE_URL` | API base URL (defaults to `http://localhost:5053/api`) |

---

## Running Tests

### Backend

```bash
cd backend
dotnet test
```

18 tests covering `GamesController` (ownership rules, backend timestamp setting, IGDB import deduplication) and `TokenService` (JWT claim generation).

### Frontend

```bash
cd frontend/collistable-frontend
npm test
```

13 tests covering the auth store (token decoding, expiry, localStorage persistence) and `ServiceBase` (401 handling, redirect to login, Authorization header attachment).

---

## CI

GitHub Actions runs both test suites automatically on every push to `main`, `feature/**`, and `dev/**` branches, and on all pull requests targeting `main`.
