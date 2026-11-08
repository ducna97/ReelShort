# ReelShort Agent Guide

## Scope

These instructions apply to the entire repository. Keep changes focused on the requested task and preserve unrelated user changes already present in the worktree.

## Repository layout

- `ReelShort.Domain`: domain entities, enums, and shared domain primitives. It must not depend on the other projects.
- `ReelShort.Application`: application services, DTOs, interfaces, validation, and configuration models. It depends on `ReelShort.Domain`.
- `ReelShort.Infrastructure`: EF Core persistence, repositories, migrations, Redis, and external infrastructure implementations. It depends on `ReelShort.Application`.
- `ReelShort.API`: ASP.NET Core composition root, controllers, middleware, authentication, Swagger, and dependency injection.
- `ReelShort.Web`: React 19 + TypeScript frontend built with Vite 7.

Respect the existing dependency direction. Keep controllers thin, put business behavior in Application services, and keep persistence concerns in Infrastructure.

## Local development

Backend requirements: .NET 10 SDK, PostgreSQL, and Redis.

```bash
dotnet restore ReelShort.slnx
dotnet build ReelShort.slnx
dotnet run --project ReelShort.API/ReelShort.API.csproj
```

The development API runs at `http://localhost:5234`; Swagger is available at `/swagger`.

Frontend requirements: a Node.js version supported by Vite 7.

```bash
cd ReelShort.Web
npm install
npm run dev
```

The Vite development server runs at `http://localhost:5173`.

## Configuration and secrets

- Treat `ReelShort.API/appsettings.json` as a template. Put local credentials in the ignored `appsettings.Development.json`, .NET user secrets, or environment variables.
- Frontend configuration belongs in `ReelShort.Web/.env.local`; use `ReelShort.Web/.env.example` as the template.
- Only variables prefixed with `VITE_` are exposed through `import.meta.env`. They are embedded in the browser bundle and must never contain secrets.
- OAuth client IDs and Facebook app IDs may be exposed to the browser. OAuth client secrets, Facebook app secrets, JWT keys, database passwords, and API secrets must remain server-side.
- Do not commit populated local configuration files. If a new local-only file is introduced, ensure it is ignored.

## Authentication flow

- Frontend auth UI lives in `ReelShort.Web/src/features/auth/AuthModal.tsx`.
- Browser SDK integration lives in `ReelShort.Web/src/features/auth/socialAuth.ts`.
- HTTP auth calls are centralized in `ReelShort.Web/src/api.ts`.
- Google Identity Services returns an ID token (`credential`) to the frontend. The frontend sends it as `accessToken` to `POST /api/auth/google-login`.
- Google `VITE_GOOGLE_CLIENT_ID` and backend `ExternalLoginSettings:Google:ClientId` must be identical because the API validates the token audience.
- Facebook follows the same boundary: obtain the browser access token in `socialAuth.ts`, then validate/use it through the API.
- Preserve loading, unmount, cancellation, and error handling in `AuthModal.tsx`. User-facing frontend messages should remain in Vietnamese unless the product language changes.

## Backend conventions

- Nullable reference types and implicit usings are enabled.
- Prefer dependency injection through constructors and asynchronous I/O APIs.
- Use DTOs at API/application boundaries rather than returning EF entities directly.
- Return responses through the existing controller response helpers and global middleware conventions.
- Register new services and configuration in `ReelShort.API/Extensions/ServiceExtensions.cs`; pipeline changes belong in `PipelineExtensions.cs`.
- Never log passwords, JWTs, refresh tokens, OAuth tokens, secrets, or full sensitive request bodies.

For schema changes, update the model and create an EF Core migration rather than editing generated migration metadata manually:

```bash
dotnet ef migrations add <MigrationName> \
  --project ReelShort.Infrastructure/ReelShort.Infrastructure.csproj \
  --startup-project ReelShort.API/ReelShort.API.csproj
```

## Frontend conventions

- Use TypeScript function components and hooks; keep reusable UI in `src/components` and feature-specific code in `src/features`.
- Keep API base URL and request handling centralized in `src/api.ts`.
- Add new Vite environment variable types to `src/vite-env.d.ts` and document them in `.env.example`.
- Reuse the existing `Button` and `Modal` components and established CSS class conventions before introducing new abstractions.
- Do not edit generated or dependency directories: `node_modules`, `dist`, `bin`, or `obj`.

## Subagent delivery workflow

For requests that change application code, coordinate these repository-local skills in order:

`Requirement/Plan → Implementation → Testing → Code Review → Fix → Re-review → Done`

- Requirement/Plan subagent: read `skills/requirement-analysis/SKILL.md` and create the task plan under `plans/`.
- Implementation/Fix subagent: read `skills/implementation/SKILL.md` and implement only the accepted plan or verified feedback.
- Testing subagent: read `skills/testing/SKILL.md`, use an independent context, and never modify production code.
- Code Review/Re-review subagent: read `skills/code-review/SKILL.md`, use an independent context, and never modify code.

Route models dynamically instead of binding a model name to a skill. Choose based on task complexity, risk, tool requirements, availability, latency, and cost. Prefer stronger reasoning for ambiguous requirements, architecture/security decisions, disputed failures, and final review. Prefer an efficient coding-capable model for bounded implementation, deterministic test execution, and localized fixes. Escalate when evidence is weak or complexity is higher than expected.

Run dependent stages sequentially and keep only one code-writing subagent active. Independence requires a separate agent/context from the code author, but not necessarily a different model. Return verified test failures and blocking review findings to the Implementation subagent, then send the final diff back for Re-review. Stop after three unsuccessful Fix/Re-review cycles and ask the user for direction.

Do not declare Done until acceptance criteria are satisfied, relevant validation passes, and Code Review approves the final state. This workflow does not authorize commits, pushes, deployments, destructive operations, credential changes, or unrelated edits.

## Context handoff

For work that may span a context compaction, agent change, or later session, read and follow `skills/context-handoff/SKILL.md`. Create or refresh its task snapshot when context usage approaches 65-70%; if an exact usage indicator is unavailable, checkpoint at the earliest reliable compaction warning and before loading another large artifact. Continue working after the checkpoint unless the user requested a pause or session transition.

## Validation before handoff

Run checks relevant to the changed area:

```bash
dotnet build ReelShort.slnx
npm --prefix ReelShort.Web run build
npm --prefix ReelShort.Web run format:check
```

There is currently no test project or frontend test script. For non-trivial business logic, add focused tests when practical and document any verification that could not be run because required services or credentials were unavailable.
