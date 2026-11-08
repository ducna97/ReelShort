using ReelShort.Application.DTOs.Auth;
using ReelShort.Domain.Entities;

namespace ReelShort.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string accessToken, Guid currentUserId, string? refreshToken = null);
    Task<AuthResponse> ExternalLoginAsync(ValidatedExternalIdentity identity);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}