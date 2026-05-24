namespace ReelShort.Application.DTOs.Auth;

public class AuthResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }
}

// Facebook Response Models
public class FacebookDebugTokenResponse
{
    public FacebookDebugTokenData? Data { get; set; }
}

public class FacebookDebugTokenData
{
    [System.Text.Json.Serialization.JsonPropertyName("is_valid")]
    public bool IsValid { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}

public class FacebookUserInfo
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public FacebookPicture? Picture { get; set; }
}

public class FacebookPicture
{
    public FacebookPictureData? Data { get; set; }
}

public class FacebookPictureData
{
    public string? Url { get; set; }
}