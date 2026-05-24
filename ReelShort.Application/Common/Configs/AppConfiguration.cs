namespace ReelShort.Application.Common.Configs;

public class AppConfiguration
{
    public JwtSettings Jwt { get; set; } = null!;
    public MediaSettings Media { get; set; } = null!;
    public ExternalLoginSettings LoginSettings { get; set; } = null!;
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

public class ExternalLoginSettings
{
    public GoogleSettings Google { get; set; } = null!;
    public FacebookSettings Facebook { get; set; } = null!;
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class FacebookSettings
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}