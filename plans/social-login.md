# Google and Facebook login

## Outcome

Complete and trial the existing Google and Facebook login integration. Provider credentials must be validated server-side, successful login must return the existing ReelShort session response, and real-provider trial results must be reported truthfully.

Status: READY for implementation. Live provider trials still require valid developer-app configuration, test accounts, PostgreSQL, Redis, network access, and human consent.

## Scope

In scope:

- Google Identity Services credential through `POST /api/auth/google-login`.
- Facebook JavaScript SDK token through `POST /api/auth/facebook-login`.
- Stable provider/subject identity persistence and safe account resolution.
- Reliable SDK initialization, cancellation, retry, and submission locking.
- Focused regression coverage, local setup documentation, and trial checklist.

Out of scope:

- Account-linking UI or automatic merging by email.
- Password/session redesign, deployment, provider dashboard mutation, or credential rotation.
- Claims that a live provider worked when credentials or human consent were unavailable.

## Existing state and gaps

- `AuthModal.tsx`, `socialAuth.ts`, and `api.ts` already contain partial frontend flows.
- `AuthController` already validates Google signatures/audience but does not require a subject, usable email, or `email_verified`.
- Facebook checks only `debug_token.is_valid`; it does not bind the token to the configured app, verify expiry, or match `debug_token.user_id` with `/me.id`.
- Facebook tokens and app secret currently occur in request URLs, which can leak through outbound HTTP logging.
- `AuthService` resolves external users by email only; no stable provider subject is persisted.
- Loading the Facebook SDK only after a click can lose browser user activation and cause popup blocking.
- Failed SDK promises remain cached, and overlapping/stale callbacks are insufficiently guarded.
- `ExternalAuthRequest` permits an empty token.
- No automated test project currently exists, and a Vite build is not a substitute for explicit TypeScript checking.

## Decisions

1. Preserve existing routes, `{ accessToken }` request shape, response envelope, and application token issuance.
2. Identify external accounts by `(provider, subject)`, using Google's validated `sub` and Facebook's validated user ID.
3. Add nullable external provider/subject fields with a unique composite index.
4. Never silently attach a provider identity to an existing account based only on matching email.
5. On email collision, return a controlled Vietnamese error and leave the existing account unchanged.
6. Require a usable, verified Google email and a usable Facebook email for initial registration.
7. Preserve legacy accounts without guessing provider subjects. Account linking/recovery is separate work.
8. Keep the current Facebook SDK/API version unless implementation evidence requires a compatible change.
9. Never expose provider tokens, secrets, application tokens, refresh tokens, passwords, or sensitive auth bodies in logs or client errors.

## Acceptance criteria

- A configured Google or Facebook identity can create a ReelShort account and receive the existing application session response.
- Repeated login with the same provider subject returns the same application user.
- Matching email with a different or unbound provider subject cannot access or overwrite an existing account.
- Google rejects missing/malformed/expired/wrong-audience tokens and missing subject/email/unverified email.
- Facebook rejects invalid, wrong-app, expired, missing/mismatched subject, and missing-email tokens.
- SDK/configuration/network/cancellation errors produce retryable Vietnamese UI errors.
- Facebook SDK is ready before an enabled button calls `FB.login` synchronously from the user click.
- Duplicate callbacks and overlapping email/social submissions cannot create concurrent login attempts.
- Closing the modal prevents stale callbacks from changing UI or completing login.
- Failed SDK loading can be retried without reloading the page.
- Local configuration remains uncommitted and templates contain no secrets.
- Relevant backend/frontend builds, TypeScript checks, tests, and formatting pass.
- Independent review approves the final diff.

## Implementation plan

1. Configuration and documentation
   - Update `.gitignore` for frontend local env and generated directories while preserving `.env.example`.
   - Document provider setup, local execution, troubleshooting, and separate Google/Facebook trial results.
   - Do not overwrite populated `appsettings.Development.json`.

2. Stable external identity
   - Add nullable external provider and subject fields to `User`.
   - Configure bounded lengths and a unique composite index.
   - Generate an additive EF migration; do not auto-apply it to an unidentified database or backfill identity from email.

3. Provider validation
   - Add an Application identity DTO and validator abstraction.
   - Implement Google and Facebook validators in Infrastructure.
   - Validate Google audience/lifetime/issuer plus required claims.
   - Validate Facebook app ID, user ID, expiry/data-access expiry, then require `/me.id` to match and return email.
   - Use bounded HTTP timeouts/cancellation and prevent secret-bearing outbound requests from being logged.

4. Application/API orchestration
   - Resolve repeat users only through provider/subject.
   - Create new identities atomically and reject email collisions without mutation.
   - Keep controllers thin, validate empty requests, return safe Vietnamese errors, and pass cancellation.
   - Register settings, validators, and dedicated HTTP clients through existing DI extensions.

5. Frontend lifecycle
   - Prepare the Facebook SDK before enabling login; keep `FB.login` synchronous with the click.
   - Add bounded script loading and reset failed cached promises.
   - Prevent stale Google handlers and callbacks from old modal instances.
   - Use a synchronous shared lock for email and social submissions and release it on every path.
   - Add abort/cancellation support where needed while preserving existing API contracts and UI style.

6. Logging and tests
   - Preserve existing auth-body redaction and verify malformed bodies cannot fall back to raw logging.
   - Add focused backend tests for provider validation, identity resolution, collisions, expiry, and no mutation on failure when practical.
   - If frontend test harness expansion is disproportionate, record reproducible manual lifecycle checks instead of inventing coverage.

## Validation

```bash
dotnet restore ReelShort.slnx
dotnet build ReelShort.slnx
dotnet test ReelShort.slnx --no-build
npm --prefix ReelShort.Web run build
npm --prefix ReelShort.Web exec -- tsc --noEmit -p tsconfig.json
npm --prefix ReelShort.Web run format:check
```

Migration generation:

```bash
dotnet ef migrations add AddExternalLoginIdentity \
  --project ReelShort.Infrastructure/ReelShort.Infrastructure.csproj \
  --startup-project ReelShort.API/ReelShort.API.csproj
```

Inspect the migration before applying it. Database update is permitted only for an explicitly identified disposable/local development database.

## Manual trial prerequisites

- Google web OAuth client with the exact frontend origin authorized.
- `VITE_GOOGLE_CLIENT_ID` identical to backend `ExternalLoginSettings:Google:ClientId`.
- Facebook developer app/tester with matching frontend App ID and backend App ID; App Secret remains server-side.
- PostgreSQL, Redis, valid JWT configuration, provider network access, and human completion of login/MFA/consent.
- Never paste credentials or tokens into chat or commit them.

## Risks

- Existing social users do not have provider subjects and cannot be safely auto-migrated; preserve them and fail collisions with guidance.
- Provider/browser policy or dashboard configuration may block a live trial even when deterministic application checks pass.
- Existing unrelated worktree changes in auth files and the untracked frontend must be preserved.
- Existing browser session storage is unchanged and outside this task.
