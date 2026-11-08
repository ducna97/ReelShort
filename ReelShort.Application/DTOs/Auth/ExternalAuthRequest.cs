using System.ComponentModel.DataAnnotations;

namespace ReelShort.Application.DTOs.Auth;

public class ExternalAuthRequest
{
    /// <summary>
    /// Token from Google/Facebook SDK on client side
    /// </summary>
    [Required(ErrorMessage = "Access token is required.")]
    [MinLength(1, ErrorMessage = "Access token is required.")]
    public string AccessToken { get; set; } = string.Empty;
}
