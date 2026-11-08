using System.Text.Json.Serialization;

namespace ReelShort.Infrastructure.Models.Facebook;

internal sealed class FacebookDebugTokenData
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; init; }

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; init; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; init; }

    [JsonPropertyName("data_access_expires_at")]
    public long DataAccessExpiresAt { get; init; }
}
