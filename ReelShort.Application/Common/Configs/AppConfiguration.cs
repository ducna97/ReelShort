namespace ReelShort.Application.Common.Configs;

public class AppConfiguration
{
    public JwtSettings Jwt { get; set; } = null!;
    public MediaSettings Media { get; set; } = null!;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}

public class MediaSettings
{
    // Configure Cloudinary or AWS S3 to save videos
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string VideoFolder { get; set; } = "reelshort/videos";
    public string AvatarFolder { get; set; } = "reelshort/avatars";
}