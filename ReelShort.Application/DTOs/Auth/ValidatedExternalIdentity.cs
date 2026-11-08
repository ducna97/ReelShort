namespace ReelShort.Application.DTOs.Auth;

public sealed record ValidatedExternalIdentity(
    string Provider,
    string Subject,
    string Email,
    string Name,
    string? AvatarUrl);
