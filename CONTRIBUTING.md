# Collistable — Development Standards

This document defines the conventions and standards for this project. All contributions should follow these guidelines.

---

## 1. Security

- **Never trust client input.** Validate at every API boundary. For `record` input DTOs, non-nullable constructor parameters enforce presence at binding time — that covers required fields. For business-rule validation (max length, allowed values, numeric ranges), validate explicitly in the controller and return `BadRequest(message)`. Do not rely on the client to send well-formed data.

- **HTTPS in production.** `UseHttpsRedirection()` is already enabled for non-development environments in `Program.cs`. Never remove it, and never move it inside a development-only block.

- **CORS must stay restrictive.** The CORS policy in `Program.cs` allows only the configured frontend origin. Never change it to `AllowAnyOrigin()` — not even temporarily for debugging.

- **OAuth flows must include a CSRF `state` parameter.** Frontend generates `crypto.randomUUID()`, stores it in `sessionStorage`, appends it to the authorization URL, and the callback validates it before calling the backend. Clear the state from `sessionStorage` immediately after reading it, whether the check passes or fails. Reject and redirect to `/login` on mismatch or if state is absent.

- **Secrets stay in `.env` / environment variables.** Never hard-code credentials, client secrets, or JWT keys in source files. The `.gitignore` already excludes `.env` — keep it that way.

- **Authorization on every protected endpoint.** All controllers that return or mutate user data must carry `[Authorize]`. Always filter queries by the authenticated user's ID from JWT claims. Never trust a user-supplied ID in the request body or query string.

- **`CurrentUserId` must not silently fall back to 0.** The correct pattern throws if the claim is missing — which should never happen on an `[Authorize]` endpoint, but failing loudly is safer than silently querying user 0:
  ```csharp
  private int CurrentUserId =>
      int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
          ? id
          : throw new InvalidOperationException("NameIdentifier claim is missing");
  ```

- **No SQL injection.** Always use EF Core parameterised queries — never raw string interpolation in queries.

- **No XSS.** Vue templates auto-escape interpolated values. Never use `v-html` or `innerHTML` with user-supplied content.

---

## 2. Consistency

### Frontend — Display strings
- Every user-visible string goes in `src/library/strings/<domain>.ts` as a `SCREAMING_SNAKE_CASE` key in the `as const` object, then re-exported from `CommonStrings.ts` under the matching namespace.
- To add a string: add the key to the right domain file (e.g. `auth.ts`, `games.ts`). Create a new domain file + export in `CommonStrings.ts` only if no existing domain fits.
- Use it in templates/script as `strings.DOMAIN.KEY`. No string literals anywhere else.

### Frontend — CSS classes (TypeScript library)
- Every Bootstrap class used in a template goes in `src/library/styles/<domain>.ts` as a `SCREAMING_SNAKE_CASE` key mapping to a Bootstrap class string (e.g. `"btn btn-dark"`), then re-exported from `CommonStyles.ts`.
- To add a class: add the key to the right domain file. Create a new domain file + export only if no existing domain fits.
- Apply in templates via `:class="styles.DOMAIN.KEY"`. No `<style scoped>` blocks. No raw class strings in templates.
- No `style=""` attributes on structural elements. Minimal inline styles are acceptable only on presentational elements like inline SVG icons where no Bootstrap utility applies.
- This project uses Bootstrap — always prefer a Bootstrap utility class over writing custom CSS.

### Frontend — SCSS architecture (7-1 + ITCSS)
The SCSS source lives in `src/assets/styles/` and follows the **7-1 pattern** organised by **Inverted Triangle CSS (ITCSS)** specificity order. `main.scss` is the single entry point — it imports every partial in layer order from least-specific to most-specific.

**Folder structure and what belongs where:**

```
src/assets/styles/
├── main.scss                   ← single entry point, imports only
├── abstracts/                  ← no CSS output (variables, mixins)
│   ├── _variables.scss         ← Bootstrap overrides + project sizing tokens
│   └── _mixins.scss            ← reusable mixins and functions
├── base/                       ← element-level defaults, no classes
│   ├── _reset.scss             ← box-sizing and low-specificity resets
│   └── _base.scss              ← bare HTML element defaults (img, a, etc.)
├── layout/                     ← sitewide structural regions
│   └── _layout.scss            ← .app-wrapper, .app-nav, .page-content, etc.
├── components/                 ← reusable UI pieces used on multiple pages
│   ├── _card.scss              ← .card-img-cover (shared by GameList + Search)
│   └── _toast.scss             ← .toast-container
├── pages/                      ← styles scoped to a single route/page
│   ├── _game-list.scss
│   ├── _game-create-edit.scss
│   ├── _game-details.scss
│   └── _login.scss
├── themes/                     ← create when a second visual theme is needed
├── vendors/                    ← create for third-party overrides beyond Bootstrap variables
└── utilities/                  ← highest-specificity helpers, always last
    └── _utilities.scss
```

**ITCSS layer order in `main.scss` (must not be reordered):**
1. `abstracts/variables` — Bootstrap variable overrides (must precede Bootstrap import)
2. `abstracts/mixins` — mixin definitions (no CSS output)
3. `bootstrap/scss/bootstrap` — vendor CSS (Generic → Components)
4. `base/reset`, `base/base` — project-level element defaults
5. `layout/layout` — structural regions
6. `components/*` — reusable UI pieces
7. `pages/*` — page-specific styles
8. `utilities/utilities` — overrides (always last, highest specificity)

**File naming rules:**
- All SCSS partials use `_kebab-case.scss` — lowercase, hyphen-separated, leading underscore.
- Never use PascalCase or camelCase for SCSS files (`_GameList.scss` ✗ → `_game-list.scss` ✓).
- `main.scss` has no leading underscore — it is the entry point, not a partial.

**When to add a new SCSS file:**
- New reusable UI component → `components/_component-name.scss`
- New page/route → `pages/_page-name.scss`
- New project-wide variable or sizing token → `abstracts/_variables.scss`
- New reusable mixin → `abstracts/_mixins.scss`
- Then add the `@import` line to `main.scss` in the correct layer section.

**`@use` in partials:**
- Partials that reference variables or mixins declare their own `@use` at the top: `@use '../abstracts/variables' as v;` / `@use '../abstracts/mixins' as m;`.
- `main.scss` uses `@import` (required for Bootstrap variable override mechanism). This is the established, Bootstrap-compatible pattern — do not migrate to `@use` in `main.scss` without also migrating the Bootstrap import strategy.

### Frontend — Services
- Services live in `src/services/` and export a single named object with arrow function methods: `export const fooService = { getAll: () => ServiceBase.get<Foo[]>("/foo"), ... }`.
- All methods delegate to `ServiceBase.get / post / put / delete`. Never call `fetch` directly from a component or store.
- Response types are always explicitly generic: `ServiceBase.get<Foo[]>`.

### Frontend — Stores
- Use the Pinia **composition API setup function** pattern: `defineStore("name", () => { ... return { ... } })`.
- Never use the options API (`defineStore("name", { state, actions, getters })`).
- `localStorage` access belongs only in `src/stores/auth.ts`. No other file touches `localStorage`.

### Frontend — Types
- Domain types (API shapes, entities) live in `src/types/` and are declared as `interface`.
- Use `type` for union types, intersection types, and aliases: `type OwnedFilter = "all" | "owned" | "not-owned"`. Use `interface` for object shapes. Do not use `interface` for unions.
- Inline response shapes used only within a single service file may stay local but must still follow the `interface` / `type` rule above.

### Backend — Controllers
- Use **primary constructor syntax**: `public class FooController(AppDbContext db) : ControllerBase`.
- Every controller that serves user data carries `[Authorize]`, `[ApiController]`, and `[Route("api/[controller]")]`.
- All actions return `async Task<IActionResult>` and accept `CancellationToken cancellationToken` as the last parameter. Pass it to all EF queries and outbound HTTP calls.

### Backend — DTOs
- **Input DTOs** (request bodies): `record` types — `public record CreateFooRequest(string Name, int Value);`. Non-nullable parameters enforce presence at binding time; add explicit business-rule validation in the controller for anything beyond that.
- **External API response models** (deserialised from third-party JSON): plain class with `[JsonPropertyName]` for snake_case mapping.
- No `[Required]`, `[MaxLength]`, or other data annotation attributes on DTOs — validation logic lives in the controller.

### Backend — Data models
- Plain C# classes with auto-properties. No data annotations.
- Non-nullable `string` properties default to `= ""`. `DateTime` properties default to `= DateTime.UtcNow`.
- EF configuration (indexes, relationships, cascade behaviour) goes in `AppDbContext.OnModelCreating`, not on the model class.
- Every schema change requires an EF migration (`dotnet ef migrations add <Name>`). Never use `EnsureCreated` to work around a missing migration locally.

### Backend — Services
- Constructor injection with `private readonly` underscore-prefixed fields.
- All I/O methods are `async Task<T>` and suffixed with `Async`.
- Registered in `Program.cs` with the appropriate lifetime (`AddScoped` for DB-touching services).

### Backend — Error responses
- Use the standard ASP.NET Core result helpers: `BadRequest(message)` → 400, `Unauthorized(message)` → 401, `Forbid()` → 403, `NotFound()` → 404, `Conflict(message)` → 409.
- This project intentionally does not use RFC 7807 `ProblemDetails` to keep response shapes simple. Keep parity with existing controllers — do not introduce a structured error envelope.

---

## 3. Efficiency

- **One `HttpClient` per request.** Call `_httpClientFactory.CreateClient(...)` once at the top of the action method and pass it to private helpers — never call it inside the helpers themselves.
- **`static readonly` for constant allocations.** `JsonSerializerOptions`, compiled regexes, and similar objects that don't vary per request must be `private static readonly` fields.
- **`AsNoTracking()` for read-only queries.** Any EF query in a `GET` endpoint that does not need to track changes for a subsequent `SaveChangesAsync` call should use `.AsNoTracking()`. This avoids the overhead of the EF change tracker.
- **No redundant DB round-trips.** Combine lookups where possible; avoid N+1 query patterns.
- **`async`/`await` all the way down.** Never block with `.Result` or `.Wait()`.
- **TypeScript strict mode is on.** `strict: true`, `noUnusedLocals`, and `noUnusedParameters` are all enabled in `tsconfig.app.json`. Every value must be typed; no `any`; no dead variables or parameters.

---

## 4. Tests

### When tests are required
Any change to a service, store, controller, or component that contains logic **must** be accompanied by new or updated tests. This includes:
- New API endpoints → test happy path, auth/ownership enforcement, and relevant error cases
- New frontend services → test correct endpoint, correct payload, and error propagation
- New frontend components with logic → test each meaningful branch (`onMounted`, conditional navigation, toast on error, state validation)
- Changed behaviour in existing code → update the affected tests before marking the task done

### Frontend (Vitest + @vue/test-utils)
- Test files live alongside source files: `Foo.spec.ts` next to `Foo.ts` / `Foo.vue`.
- Use real Pinia stores with `setActivePinia(createPinia())` — do not mock stores.
- Mock only external boundaries: `vi.mock` for service modules (`../services/AuthService`), `globalThis.fetch` for raw HTTP.
- Mock `../router` in any spec that imports `ServiceBase` to avoid circular dependency errors.
- Always `await flushPromises()` after mounting components with async `onMounted` logic.
- Always call `sessionStorage.clear()`, `localStorage.clear()`, and `vi.clearAllMocks()` in `beforeEach`.
- Use top-level `await import(...)` after `vi.mock(...)` calls to ensure mocks are in place before the module loads.

### Backend (xUnit + EF In-Memory)
- Test project: `Collistable.Api.Tests` using xUnit.
- Use `[Fact]` for single-case tests. Use `[Theory]` with `[InlineData]` for parameterised tests (e.g. testing the same endpoint with multiple invalid inputs).
- Create a fresh in-memory database per test: `new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options` — unique name prevents test cross-contamination.
- **Known limitation:** The EF in-memory provider does not enforce unique index constraints. Do not write tests that rely on unique constraint violations to prove correctness — those must be verified against a real database.
- Provide helper factory methods (`CreateDb()`, `CreateController(userId)`) at the top of each test class.
- Mock the authenticated user via `ClaimsPrincipal` with `NameIdentifier` claim set to the test user's ID.
- Test naming convention: `MethodName_Scenario_ExpectedResult` (e.g. `GetGames_WhenOtherUsersGamesExist_ReturnsOnlyOwnGames`).
- New endpoints need tests for: returns correct data for owner, returns empty/404 for non-owner, returns 401 when unauthenticated.

---

## 5. CI

Both jobs in `.github/workflows/ci.yml` must pass on every branch push and PR to `main`.

- **Backend:** `dotnet restore` → `dotnet test` in `backend/Collistable.Api.Tests`
- **Frontend:** `npm ci` → `npm test` (Vitest) in `frontend/collistable-frontend`

Never merge with a failing CI run. If a test was already failing before your change, fix it as part of your change.

---

## 6. Deployment

### Infrastructure
- **Frontend** — hosted on Netlify (auto-deploys from `main`).
- **Backend API** — hosted on Railway (.NET service).
- **Database** — PostgreSQL on Railway (same project as the API); SQL Server locally.

### Environment variables
**Netlify (frontend build-time):**
| Variable | Purpose |
|---|---|
| `VITE_API_BASE_URL` | Railway API URL |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID |
| `VITE_GITHUB_CLIENT_ID` | GitHub OAuth client ID (production app) |

`VITE_*` variables are baked into the JS bundle at build time. Adding or changing one requires updating the Netlify build settings **and triggering a redeploy** — changing it only in the code has no effect until the next build.

**Railway (backend runtime):**
| Variable | Purpose |
|---|---|
| `JWT__SECRET` | JWT signing key |
| `GOOGLE__CLIENTID` | Google OAuth client ID |
| `GITHUB__CLIENTID` | GitHub OAuth client ID (production app) |
| `GITHUB__CLIENTSECRET` | GitHub OAuth client secret (production app) |
| `ALLOWED_ORIGIN` | Exact Netlify production URL (used by CORS policy) |
| `USE_POSTGRES` | Set to `true` on Railway to switch EF Core to Npgsql |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string (Railway) |

`ALLOWED_ORIGIN` must exactly match the Netlify site URL (e.g. `https://collistable.netlify.app`) — no trailing slash, correct protocol. A mismatch silently breaks all API requests from the frontend.

### Dual database providers
`Program.cs` selects the EF Core provider at startup based on `USE_POSTGRES`:
- **Local** (`USE_POSTGRES` absent or not `"true"`) → `UseSqlServer` (SQL Server)
- **Railway** (`USE_POSTGRES=true`) → `UseNpgsql` (PostgreSQL)

EF migrations live in a single `Migrations/` folder and must be compatible with both providers. Rules:
- Never use SQL Server-specific migration operations (e.g. `sql("..."` with T-SQL syntax, `uniqueidentifier` column types). Stick to provider-agnostic EF operations.
- Generate migrations locally against SQL Server (`dotnet ef migrations add <Name>`), then verify the migration applies cleanly on Railway (PostgreSQL) before merging.
- If a migration fails on PostgreSQL after being generated on SQL Server, fix the migration SQL — do not work around it with `EnsureCreated` or manual schema edits.

### SPA routing
Netlify serves the frontend as a SPA. All routes must fall through to `index.html`. This is handled by the `_redirects` file (or `netlify.toml` rewrite rule) in the frontend build output — do not remove it.

### OAuth providers (GitHub)
GitHub OAuth Apps allow only **one callback URL per app**. Two separate GitHub OAuth Apps are required:
- **Dev app** — callback URL: `http://localhost:5173/auth/github/callback`
- **Prod app** — callback URL: `https://<netlify-site-url>/auth/github/callback`

Local development uses the dev app credentials (stored in `frontend/.env.local` and `backend/.env`). Production uses the prod app credentials (stored in Netlify and Railway environment variables). Never use production OAuth credentials in local development.

### Checklist for changes that affect deployment
- New `VITE_*` env var → add to Netlify build settings, redeploy.
- New backend secret or config value → add to Railway environment variables.
- New OAuth provider → create two OAuth Apps (dev + prod), update both Netlify and Railway, update this table.
- CORS origin change → update `ALLOWED_ORIGIN` in Railway.
