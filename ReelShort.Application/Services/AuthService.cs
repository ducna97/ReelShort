using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ReelShort.Application.DTOs.Auth;
using ReelShort.Application.Interfaces;
using ReelShort.Domain.Entities;

namespace ReelShort.Application.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    
    public AuthService(ILogger<AuthService> logger,
        IRepository<User> userRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IJwtService jwtService,
        ITokenBlacklistService tokenBlacklistService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _tokenBlacklistService = tokenBlacklistService;
    }
    
    #region Main methods
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("[RegisterAsync] Start to register user with email: {Email}", request.Email);
            
            // Check if email already exists
            var userExisted = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (userExisted != null)
            {
                _logger.LogError($"[RegisterAsync] Email already exists, email: {request.Email}.");
                throw new Exception($"[RegisterAsync] Email already exists.");
            }
            
            // Check if username already exists
            var usernameExists = await _userRepository.AnyAsync(u => u.Username == request.Username);
            if (usernameExists)
            {
                _logger.LogError($"[RegisterAsync] Username already exists, email: {request.Username}.");
                throw new Exception($"[RegisterAsync] Username already exists.");
            }
            
            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.Username,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = request.Username,
                UpdatedBy = request.Username,
            };
            
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            
            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Username);
            
            _logger.LogInformation("[RegisterAsync] End to register user with email: {Email}.", request.Email);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                Token = token,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"[RegisterAsync] Error: {ex.Message}", ex);
            throw new Exception($"[RegisterAsync] An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("[LoginAsync] Start to login with email: {Email}.", request.Email);
            
            // Find user by email
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                _logger.LogError($"[LoginAsync] Email {request.Email} not found.");
                throw new Exception("[LoginAsync] Invalid email or password.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("[LoginAsync] Invalid credentials for email: {Email}.", request.Email);
                throw new Exception("[LoginAsync] Invalid credentials for email.");
            }

            _logger.LogInformation("[LoginAsync] End to login with email: {Email}.", request.Email);
            
            return await BuildAuthResponseAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[LoginAsync] Error: {ex.Message}", ex);
            throw new Exception($"[LoginAsync] An error occurred during login: {ex.Message}");
        }
    }
    
    public async Task LogoutAsync(string accessToken, Guid currentUserId, string? refreshToken = null)
    {
        try
        {
            _logger.LogInformation("[LogoutAsync] Start logout.");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var expiry = jwtToken.ValidTo - DateTime.UtcNow;

            if (expiry > TimeSpan.Zero)
            {
                await _tokenBlacklistService.BlacklistTokenAsync(accessToken, expiry);
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshTokenHash = HashRefreshToken(refreshToken);
                var existingRefreshToken = await _refreshTokenRepository.FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash
                    && x.UserId == currentUserId, true);

                if (existingRefreshToken != null && !existingRefreshToken.IsRevoked)
                {
                    existingRefreshToken.IsRevoked = true;
                    existingRefreshToken.RevokedAt = DateTime.UtcNow;
                    existingRefreshToken.UpdatedAt = DateTime.UtcNow;
                    await _refreshTokenRepository.UpdateAsync(existingRefreshToken);
                    await _refreshTokenRepository.SaveChangesAsync();
                }
            }

            _logger.LogInformation("[LogoutAsync] Logout successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LogoutAsync] Error: {Message}", ex.Message);
            throw new Exception($"[LogoutAsync] {ex.Message}");
        }
    }

    public async Task<AuthResponse> ExternalLoginAsync(string email, string name, string? avatarUrl, string provider)
    {
        try
        {
            _logger.LogInformation($"[ExternalLoginAsync] Start to login with email: {email}, provider: {provider}.");
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N")[..4],
                    Email = email,
                    DisplayName = name,
                    AvatarUrl = avatarUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = provider,
                    UpdatedBy = provider,
                };
                
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                
                _logger.LogInformation("[ExternalLoginAsync] Created new user from {Provider}: {UserId}", provider, user.Id);
            }

            _logger.LogInformation($"[ExternalLoginAsync] End to login with email: {email}, provider: {provider}.");
            
            return await BuildAuthResponseAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ExternalLoginAsync] Error: {ex.Message}", ex);
            throw new Exception($"[ExternalLoginAsync] An error occurred during external login: {ex.Message}");
        }
    }
    
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("[RefreshTokenAsync] Start refreshing token.");

            var incomingTokenHash = HashRefreshToken(refreshToken);
            var existingRefreshToken = await _refreshTokenRepository.FirstOrDefaultAsync(x => x.TokenHash == incomingTokenHash, true);

            if (existingRefreshToken == null)
            {
                _logger.LogError("[RefreshTokenAsync] Refresh token not found.");
                throw new Exception("Refresh token not found.");
            }

            if (!existingRefreshToken.IsActive)
            {
                _logger.LogError("[RefreshTokenAsync] Refresh token is expired or revoked.");
                throw new Exception("Refresh token is expired or revoked.");
            }

            var user = await _userRepository.GetByIdAsync(existingRefreshToken.UserId);
            if (user == null)
            {
                _logger.LogError("[RefreshTokenAsync] User not found.");
                throw new Exception("User not found.");
            }

            var newRawRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashRefreshToken(newRawRefreshToken);

            // Revoke token cũ
            existingRefreshToken.IsRevoked = true;
            existingRefreshToken.RevokedAt = DateTime.UtcNow;
            existingRefreshToken.ReplacedByTokenHash = newRefreshTokenHash;
            existingRefreshToken.UpdatedAt = DateTime.UtcNow;
            existingRefreshToken.UpdatedBy = user.Username;

            await _refreshTokenRepository.UpdateAsync(existingRefreshToken);

            // Lưu hash của token mới
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = user.Username,
                UpdatedBy = user.Username
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Username);

            _logger.LogInformation("[RefreshTokenAsync] Token refreshed successfully for user: {UserId}", user.Id);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                AccessToken = accessToken,
                RefreshToken = newRawRefreshToken,
                RefreshTokenExpiryTime = newRefreshToken.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RefreshTokenAsync] Error: {Message}", ex.Message);
            throw new Exception($"[RefreshTokenAsync] {ex.Message}");
        }
    }
    #endregion Main methods

    #region Private methods
    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
    
    private static string HashRefreshToken(string refreshToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
    
    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Username);

        // Token gốc chỉ trả về client
        var rawRefreshToken = GenerateRefreshToken();
        
        // Hash mới được lưu DB
        var refreshTokenHash = HashRefreshToken(rawRefreshToken);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiry,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = user.Username,
            UpdatedBy = user.Username
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl ?? string.Empty,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiry
        };
    }
    #endregion Private methods
}