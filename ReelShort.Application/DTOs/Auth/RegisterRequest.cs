using System.ComponentModel.DataAnnotations;

namespace ReelShort.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(50, ErrorMessage = "Username maximum 50 characters.")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password minimum 6 characters.")]
    public string Password { get; set; } = string.Empty;
}