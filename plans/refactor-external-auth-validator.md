# Refactor external authentication validator

## Objective

Replace the string provider parameter of `IExternalAuthValidator.ValidateAsync` with an enum and extract `ExternalAuthValidator`'s nested Facebook response models into separate files. Preserve authentication behavior and all HTTP, configuration, and persisted-data contracts.

## Decisions

- Add `ExternalAuthProvider` to `ReelShort.Domain.Enums` with explicit `Google = 1` and `Facebook = 2` values.
- Use the enum only for validator input and its call sites.
- Keep `ValidatedExternalIdentity.Provider`, `User.ExternalProvider`, configuration names, routes, and emitted strings unchanged.
- Put Facebook wire models under `ReelShort.Infrastructure.Models.Facebook` as internal sealed classes.
- Preserve JSON property names, nullability, cancellation, exception messages/statuses, requests, and validation ordering.
- Leave older Application DTOs and all frontend/database code unchanged.

## Acceptance criteria

- Interface and implementation accept `ExternalAuthProvider`; no string overload or string dispatch remains.
- Controller actions pass the correct enum members.
- Undefined enum values return the existing unsupported-provider error after the existing empty-token guard.
- Validator still emits provider strings exactly `Google` and `Facebook`.
- The five active Facebook response classes live outside the service with identical JSON contracts.
- No Domain/Application dependency on Infrastructure is introduced.
- Backend build passes and independent review approves the focused diff.

## Files

Add:

- `ReelShort.Domain/Enums/ExternalAuthProvider.cs`
- `ReelShort.Infrastructure/Models/Facebook/FacebookDebugTokenResponse.cs`
- `ReelShort.Infrastructure/Models/Facebook/FacebookDebugTokenData.cs`
- `ReelShort.Infrastructure/Models/Facebook/FacebookUserInfo.cs`
- `ReelShort.Infrastructure/Models/Facebook/FacebookPicture.cs`
- `ReelShort.Infrastructure/Models/Facebook/FacebookPictureData.cs`

Update:

- `ReelShort.Application/Interfaces/IExternalAuthValidator.cs`
- `ReelShort.Infrastructure/Services/ExternalAuthValidator.cs`
- `ReelShort.API/Controllers/AuthController.cs`

Use Rider semantic signature refactoring for `ValidateAsync` and its known call sites. Avoid unrelated edits.

## Validation

```bash
dotnet build ReelShort.slnx
rg -n 'IExternalAuthValidator|ExternalAuthProvider|ValidateAsync\(' \
  ReelShort.Application ReelShort.Infrastructure ReelShort.API ReelShort.Domain
rg -n 'FacebookDebugTokenResponse|FacebookDebugTokenData|FacebookUserInfo|FacebookPicture' \
  ReelShort.Infrastructure ReelShort.Application/DTOs/Auth/AuthResponse.cs
```

Status: READY.
