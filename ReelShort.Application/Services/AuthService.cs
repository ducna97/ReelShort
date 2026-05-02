using Microsoft.Extensions.Logging;
using ReelShort.Application.DTOs.Auth;
using ReelShort.Application.Interfaces;
using ReelShort.Domain.Entities;

namespace ReelShort.Application.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    
    public AuthService(ILogger<AuthService> logger,
        IRepository<User> userRepository,
        IJwtService jwtService,
        ITokenBlacklistService tokenBlacklistService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _tokenBlacklistService = tokenBlacklistService;
    }
    
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
                _logger.LogError($"[LoginAsync] Password {request.Password} not match.");
                throw new Exception("[LoginAsync] Invalid password.");
            }

            // Generate token
            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Username);

            _logger.LogInformation("[LoginAsync] End to login with email: {Email}.", request.Email);
            
            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"[LoginAsync] Error: {ex.Message}", ex);
            throw new Exception($"[LoginAsync] An error occurred during login: {ex.Message}");
        }
    }
    
    public async Task LogoutAsync(string token)
    {
        try
        {
            _logger.LogInformation("[LogoutAsync] Start to logout.");
            
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiry = jwtToken.ValidTo - DateTime.UtcNow;

            if (expiry > TimeSpan.Zero)
            {
                await _tokenBlacklistService.BlacklistTokenAsync(token, expiry);
            }
            
            _logger.LogInformation("[LogoutAsync] End to logout.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[LogoutAsync] Error: {ex.Message}", ex);
            throw new Exception($"[LogoutAsync] An error occurred during logout: {ex.Message}");
        }
    }
}