# Local Google and Facebook login setup

The application uses provider credentials only to prove identity. ReelShort validates the provider token on the API, resolves the user by provider plus provider subject, then issues its own access and refresh tokens.

## Frontend configuration

Copy `ReelShort.Web/.env.example` to `ReelShort.Web/.env.local` and replace only the public values:

```env
VITE_API_URL=http://localhost:5234/api
VITE_GOOGLE_CLIENT_ID=<web-client-id>.apps.googleusercontent.com
VITE_FACEBOOK_APP_ID=<facebook-app-id>
```

Vite embeds every `VITE_` value in the browser bundle. Never place a client secret, app secret, provider token, JWT key, or database password there. Restart Vite after changing environment variables.

## Backend configuration

Configure `ExternalLoginSettings` through ignored development configuration, .NET user secrets, or environment variables:

```text
ExternalLoginSettings:Google:ClientId
ExternalLoginSettings:Facebook:AppId
ExternalLoginSettings:Facebook:AppSecret
ExternalLoginSettings:Facebook:GraphApiVersion
```

The Google backend Client ID must exactly match `VITE_GOOGLE_CLIENT_ID`. This ID-token flow does not require a Google client secret. The Facebook App ID must match the frontend value; its App Secret stays server-side.

## Provider dashboards

Google:

- Create an OAuth client of type Web application.
- Add the exact Vite origin, normally `http://localhost:5173`, to authorized JavaScript origins.
- Configure consent/test-user access when the project is not published.

Facebook:

- Enable Facebook Login and JavaScript SDK login for the development app.
- Add the actual development host to the app's allowed domains/origins.
- Use an app administrator, developer, or tester while the app is in development mode.
- Grant the `email` permission with an account that has an accessible email.

Provider policies can change. Verify current dashboard requirements before treating a blocked popup or origin error as an application defect.

## Run locally

Inspect the existing additive migration before applying it:

```bash
ReelShort.Infrastructure/Migrations/20261108053846_AddExternalLoginIdentity.cs
```

The migration contains only the two nullable identity columns and their unique filtered index. EF originally detected stale snapshot metadata for `Videos.Status` and the `RefreshTokens` relationship; those states were already represented by the preceding migration metadata and were removed from this migration. No migration was applied during implementation.

Apply the verified migration only to an explicitly identified local/disposable database. Then start PostgreSQL, Redis, API, and frontend:

```bash
dotnet run --project ReelShort.API/ReelShort.API.csproj
npm --prefix ReelShort.Web run dev -- --host localhost --port 5173
```

## Trial checklist

Record Google and Facebook separately. Do not mark an external trial successful based only on a build or mocked response.

| Check | Google | Facebook |
| --- | --- | --- |
| SDK/button loads | Not run | Not run |
| Provider consent completes | Not run | Not run |
| First login creates a ReelShort user | Not run | Not run |
| Repeated login returns the same user | Not run | Not run |
| Cancel/failure remains retryable | Not run | Not run |
| Existing-email collision is rejected | Not run | Not run |

## Troubleshooting

- Missing frontend variable: create/update `.env.local`, then restart Vite.
- Wrong Google audience: make frontend and backend Google Client IDs identical.
- Facebook email missing: grant `email` and use an account that exposes an email to the app.
- Popup blocked: wait for the button to finish loading and initiate login directly from a click.
- API configuration error: configure the matching provider on the backend; password login remains available.
- Existing email rejected: use the sign-in method already associated with that account. Automatic account linking is intentionally out of scope.
