using ReelShort.Application.DTOs.Auth;
using ReelShort.Domain.Entities;

namespace ReelShort.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string token);
}