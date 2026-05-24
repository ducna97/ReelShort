namespace ReelShort.Application.DTOs.Auth;

public class ExternalAuthRequest
{
    /// <summary>
    /// Token from Google/Facebook SDK on client side
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
}